using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Refit;
using TinyMessenger;

namespace SlackCommander.Web.Mailgun
{
    public class WebhooksModule : NancyModule
    {
        public WebhooksModule(ITinyMessengerHub hub, IMailgunWebhooks mailgunWebhooks)
        {
            Post["/webhooks/mailgun/{webhookId}", runAsync: true] = async (_, ct) =>
            {
                var webhookId = (string)_.webhookId;
                if (webhookId.Missing())
                {
                    return HttpStatusCode.NotAcceptable.WithReason("WebhookId is missing.");
                }
                var webhook = mailgunWebhooks.Get(webhookId);
                if (webhook == null)
                {
                    return HttpStatusCode.NotAcceptable.WithReason("The webhook does not exist.");
                }

                // TODO Refactor this - should parse e-mail + invoke handlers in separate component(s)
                var subject = (string)Request.Form["subject"];
                var strippedText = (string)Request.Form["stripped-text"];
                return subject;
                return HttpStatusCode.OK;
            };
        }
    }
}