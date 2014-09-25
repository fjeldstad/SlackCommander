using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MassTransit;
using NLog;
using SlackCommander.Web.Messages;
using SlackCommander.Web.Todo;

namespace SlackCommander.Web.CommandHandlers
{
    public class SlashCommandHandler : Consumes<SlashCommand>.All
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IAppSettings _appSettings;
        private readonly IServiceBus _bus;
        private readonly ITodoService _todoService;

        public SlashCommandHandler(IAppSettings appSettings, IServiceBus bus, ITodoService todoService)
        {
            _appSettings = appSettings;
            _bus = bus;
            _todoService = todoService;
        }

        public void Consume(SlashCommand message)
        {
            string responseText = null;

            switch (message.command.ToLowerInvariant())
            {
                case "/whois":
                    responseText = HandleWhois(message);
                    break;
                default:
                    responseText = string.Format("Sorry, *{0}* is not a supported slash command.", message.command);
                    break;
            }

            _bus.Context().Respond(new SlashCommandResponse
            {
                Text = responseText
            });
        }

        private string HandleWhois(SlashCommand message)
        {
            if (!_appSettings.Get("whois:slackSlashCommandToken").Equals(message.token))
            {
                Log.Info("Blocked an unauthorized /whois slash command.");
                return null;
            }

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