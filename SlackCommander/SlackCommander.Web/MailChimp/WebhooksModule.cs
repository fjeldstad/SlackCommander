using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using NLog;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.MailChimp
{
    public class WebhooksModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public WebhooksModule(ITinyMessengerHub hub, IMailChimpWebhooks webhooks)
        {
            // This route is used by MailChimp's webhook validator.
            Get["/webhooks/mailchimp/{webhookId}"] = _ =>
            {
                var webhookId = (string)_.webhookId;
                if (webhookId.Missing())
                {
                    return HttpStatusCode.NotFound;
                }
                var webhook = webhooks.Get(webhookId);
                if (webhook == null)
                {
                    return HttpStatusCode.NotFound;
                }
                Log.Info("Got a visit from the MailChimp webhook validator (webhookId: '{0}').", webhookId);
                return HttpStatusCode.NoContent;
            };

            Post["/webhooks/mailchimp/{webhookId}"] = _ =>
            {
                var webhookId = (string)_.webhookId;
                if (webhookId.Missing())
                {
                    return HttpStatusCode.NotFound;
                }
                var webhook = webhooks.Get(webhookId);
                if (webhook == null)
                {
                    return HttpStatusCode.NotFound;
                }
                var type = (string)Request.Form["type"];
                var email = (string)Request.Form["data[email]"];

                Log.Debug("MailChimp webhook received (type: '{0}', data[email]: '{1}').", type, email);

                if (type.Missing() ||
                    !type.Equals("subscribe"))
                {
                    Log.Info("Rejected a webhook call from MailChimp (unsupported type '{0}').", type);
                    return HttpStatusCode.BadRequest.WithReason("Only notifications of type 'subscribe' are supported.");
                }
                if (email.Missing() ||
                    !email.IsValidEmail())
                {
                    Log.Info("Rejected a webhook call from MailChimp (email missing or invalid).");
                    return HttpStatusCode.BadRequest.WithReason("Expected data[email] parameter to be a valid e-mail address.");
                }

                // Send message to Slack notifying about the new subscriber.
                hub.PublishAsync(new TinyMessage<SendMessageToSlack>(new SendMessageToSlack
                {
                    Channel = webhook.SlackChannel,
                    Text = string.Format("*{0}* just signed up for the Unsampler beta! :tada:", email)
                }));

                // Request whois lookup for the new subscriber.
                hub.PublishAsync(new TinyMessage<ICommand>(new WhoisEmail
                {
                    EmailAddress = email,
                    RequestedByUser = "@slackbot",
                    RespondToChannel = webhook.SlackChannel
                }));

                return HttpStatusCode.OK;
            };
        }
    }
}