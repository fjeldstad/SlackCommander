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
                var webhookId = _.webhookId as string;
                if (webhookId.Missing())
                {
                    return HttpStatusCode.NotAcceptable.WithReason("WebhookId is missing.");
                }
                var webhook = mailgunWebhooks.Get(webhookId);
                if (webhook == null)
                {
                    return HttpStatusCode.NotAcceptable.WithReason("The webhook does not exist.");
                }


                return Request.Form;
            };
        }
    }
}