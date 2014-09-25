using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MassTransit;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using SlackCommander.Web.Whois.Messages;

namespace SlackCommander.Web.Whois
{
    public class WhoisModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IServiceBus _bus;

        public WhoisModule(IAppSettings appSettings, IServiceBus bus)
        {
            _bus = bus;

            Post["/whois"] = _ =>
            {
                var slashCommand = this.Bind<SlashCommand>();
                if (slashCommand == null ||
                    slashCommand.command.Missing())
                {
                    Log.Info("Rejected an incoming slash command (unable to parse request body).");
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse slash command.");
                }
                if (!appSettings.Get("whois:slackSlashCommandToken").Equals(slashCommand.token))
                {
                    Log.Info("Blocked an unauthorized slash command.");
                    return HttpStatusCode.Unauthorized.WithReason("Missing or invalid token.");
                }
                if (!slashCommand.command.Equals("/whois", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Info("Rejected an incoming slash command ({0} is not handled by this module).", slashCommand.command);
                    return HttpStatusCode.BadRequest.WithReason("Unsupported slash command.");
                }

                var responseText = HandleWhois(slashCommand);
                if (responseText.Missing())
                {
                    return HttpStatusCode.OK;
                }
                return responseText;
            };

            Post["/whois/fullcontact/person"] = _ =>
            {
                // Parse the request data
                var person = this.BindTo(new FullContactPersonResult());
                if (person == null ||
                    person.Result == null)
                {
                    Log.Info("Rejected webhook call from FullContact (unable to parse request body).");
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse request body.");
                }

                // Get the pending command that corresponds to the posted data
                if (string.IsNullOrWhiteSpace(person.WebhookId))
                {
                    Log.Info("Rejected a webhook call from FullContact (webhookId missing).");
                    return HttpStatusCode.BadRequest.WithReason("The webhookId property is missing from the request body.");
                }

                bus.Publish(person);
                return HttpStatusCode.OK;
            };
        }

        private string HandleWhois(SlashCommand message)
        {
            if (message.text.IsValidEmail())
            {
                _bus.Publish(new WhoisEmailRequest
                {
                    CorrelationId = Guid.NewGuid(),
                    EmailAddress = message.text,
                    RequestedByUser = message.user_name,
                    RespondToChannel =
                        message.channel_name == "directmessage" ?
                        "@" + message.user_name :
                        "#" + message.channel_name
                });
                return string.Format("Looking up e-mail address *{0}*, one moment please...", message.text);
            }

            if (message.text.CouldBeTwitterHandle())
            {
                _bus.Publish(new WhoisTwitterRequest
                {
                    CorrelationId = Guid.NewGuid(),
                    TwitterHandle = message.text,
                    RequestedByUser = message.user_name,
                    RespondToChannel =
                        message.channel_name == "directmessage"
                            ? "@" + message.user_name
                            : "#" + message.channel_name
                });
                return string.Format("Looking up Twitter handle *{0}*, one moment please...", message.text);
            }

            return "Sorry, I'm only able to work with e-mail addresses and Twitter handles.";
        }
    }
}