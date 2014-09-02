using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Nancy;
using NLog;
using Refit;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.Mailgun
{
    public class WebhooksModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public WebhooksModule(
            ITinyMessengerHub hub, 
            IMailgunWebhooks mailgunWebhooks, 
            IMailStorage mailStorage, 
            IAppSettings appSettings)
        {
            Post["/webhooks/mailgun/{webhookId}/{slackChannel}"] = _ =>
            {
                Log.Debug("Received webhook call from Mailgun.");

                // TODO Verify signature?

                var webhookId = (string)_.webhookId;
                if (webhookId.Missing())
                {
                    Log.Info("Rejected webhook call from Mailgun (WebhookId is missing).");
                    return HttpStatusCode.NotAcceptable.WithReason("WebhookId is missing.");
                }
                var webhook = mailgunWebhooks.Get(webhookId);
                if (webhook == null)
                {
                    Log.Info("Rejected webhook call from Mailgun (webhook '{0}' not found).", webhookId);
                    return HttpStatusCode.NotAcceptable.WithReason("The webhook does not exist.");
                }

                var slackChannel = "#" + ((string)_.slackChannel).TrimStart('#');

                var sender = (string)Request.Form["sender"];
                var subject = (string)Request.Form["subject"];
                var htmlBody = (string)Request.Form["body-html"];

                // Store HTML contents temporarily so that Slack can import it as an attachment
                // via "link unfurling" when the message is received.
                var mailId = Guid.NewGuid().ToString();
                mailStorage.Add(mailId, htmlBody);

                var mailUrl = appSettings.Get("slackCommander:baseUrl").TrimEnd('/') + 
                    "/temp/email/" + mailId;

                Log.Debug("E-mail from {0} to {1} temporarily stored at {2}", sender, slackChannel, mailUrl);

                // Send notification to Slack.
                hub.PublishAsync(new TinyMessage<SendMessageToSlack>(new SendMessageToSlack
                {
                    Channel = slackChannel,
                    UnfurlLinks = true,
                    Text = string.Format("E-mail from *{0}*:\n", sender) +
                           string.Format("<{0}|{1}>", mailUrl, subject)
                }));

                return HttpStatusCode.OK;
            };

            Get["/temp/email/{mailId}"] = _ =>
            {
                var mailId = (string)_.mailId;
                if (mailId.Missing())
                {
                    return HttpStatusCode.NotFound;
                }
                var htmlContents = mailStorage.GetHtmlContents(mailId);
                if (htmlContents.Missing())
                {
                    return HttpStatusCode.NotFound;
                }
                return new Nancy.Responses.TextResponse(htmlContents, "text/html", Encoding.UTF8);
            };
        }
    }
}