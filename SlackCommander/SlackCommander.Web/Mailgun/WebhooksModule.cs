using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public WebhooksModule(ITinyMessengerHub hub, IMailgunWebhooks mailgunWebhooks)
        {
            Post["/webhooks/mailgun/{webhookId}", runAsync: true] = async (_, ct) =>
            {
                Log.Debug("Received webhook call from Mailgun.");

                // TODO Verify signature

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

                // TODO Refactor this - should parse e-mail + invoke handlers in separate component(s)
                var subject = (string)Request.Form["subject"];
                var strippedText = (string)Request.Form["stripped-text"];
                if (subject.StartsWith("[TinyLetter] You have a new subscriber"))
                {
                    Log.Debug("Incoming e-mail is a notification from TinyLetter (new subscriber).");
                    const string subscriberLinePattern = "Someone just subscribed to your newsletter:";
                    var lines = strippedText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var subscriberLine = lines.FirstOrDefault(line => line.Trim().StartsWith(subscriberLinePattern));
                    if (subscriberLine != null)
                    {
                        var newSubscriber = subscriberLine.Replace(subscriberLinePattern, string.Empty).Trim();
                        if (newSubscriber.IsValidEmail())
                        {
                            Log.Debug("Notifying {0} of the new subscriber '{1}'.", webhook.SlackChannel, newSubscriber);
                            await hub.PublishAsyncUsingTask(new TinyMessage<SendMessageToSlack>(new SendMessageToSlack
                            {
                                Channel = webhook.SlackChannel,
                                Text = string.Format("*{0}* just signed up for the Unsampler beta! :tada:")
                            }));

                            Log.Debug("Publishing Whois command for '{0}'.", newSubscriber);
                            await hub.PublishAsyncUsingTask(new TinyMessage<ICommand>(new WhoisEmail
                            {
                                EmailAddress = newSubscriber,
                                RequestedByUser = "@slackbot",
                                RespondToChannel = webhook.SlackChannel
                            }));
                        }
                    }
                }
                return HttpStatusCode.OK;
            };
        }
    }
}