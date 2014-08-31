using Nancy;
using Nancy.Helpers;
using Nancy.ModelBinding;
using NLog;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.FullContact
{
    public class WebhooksModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public WebhooksModule(
            IPendingCommands pendingCommands, 
            ITinyMessengerHub hub)
        {
            Post["/webhooks/fullcontact/person"] = _ =>
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
                var command = pendingCommands.Get(person.WebhookId) as Whois;
                if (command == null)
                {
                    Log.Info("Rejected a webhook call from FullContact ('{0}' is not a pending command).", person.WebhookId);
                    return HttpStatusCode.BadRequest.WithReason("No pending command matching the webhookId could be found.");
                }
                pendingCommands.Remove(person.WebhookId);

                Log.Debug("Processing pending command '{0}' as part of responding to a webhook call from FullContact.", person.WebhookId);

                // Prepare message text
                var slackMessage = new SendMessageToSlack
                {
                    Channel = command.RespondToChannel
                };
                if (person.Result.Status != 200 ||
                    person.Result.Likelihood < 0.7)
                {
                    slackMessage.Text = string.Format(
                        "Unfortunately I'm unable to find any reliable information on who *{0}* is. " +
                        "I suggest you try <https://www.google.com/search?q={1}|Google>.",
                        command.Subject(),
                        HttpUtility.UrlEncode(command.Subject()));
                }
                else
                {
                    slackMessage.Text = string.Format(
                        "I looked up *{0}* and I'm {1:P0} sure this is the person behind it:\n\n",
                        command.Subject(),
                        person.Result.Likelihood);
                    slackMessage.Text += person.FormattedSummary();
                }

                // Post message
                hub.PublishAsync(new TinyMessage<SendMessageToSlack>(slackMessage));
                return HttpStatusCode.OK;
            };
        }
    }
}