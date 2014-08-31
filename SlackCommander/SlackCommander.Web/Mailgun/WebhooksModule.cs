using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Exceptionless.Extensions;
using Nancy;
using Refit;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.Mailgun
{
    public class WebhooksModule : NancyModule
    {
        public WebhooksModule(ITinyMessengerHub hub, IMailgunWebhooks mailgunWebhooks)
        {
            Post["/webhooks/mailgun/{webhookId}", runAsync: true] = async (_, ct) =>
            {
                Request.Body.Position = 0;
                var rawBody = new StreamReader(Request.Body).ReadToEndAsync();
                Request.Body.Position = 0;
                


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
                if (subject.StartsWith("[TinyLetter] You have a new subscriber"))
                {
                    const string subscriberLinePattern = "Someone just subscribed to your newsletter:";
                    strippedText = strippedText.NormalizeLineEndings();
                    var lines = strippedText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    return string.Join("|", lines);
                    var subscriberLine = lines.FirstOrDefault(line => line.Trim().StartsWith(subscriberLinePattern));
                    if (subscriberLine != null)
                    {
                        var newSubscriber = subscriberLine.Replace(subscriberLinePattern, string.Empty).Trim();
                        if (newSubscriber.IsValidEmail())
                        {
                            hub.Publish(new TinyMessage<ICommand>(new WhoisEmail
                            {
                                EmailAddress = newSubscriber,
                                RequestedByUser = null,
                                RespondToChannel = webhook.SlackChannel
                            }));
                        }
                    }
                }
                return strippedText;
            };
        }
    }
}