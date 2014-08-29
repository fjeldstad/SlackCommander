using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exceptionless;
using Nancy;
using Nancy.Helpers;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using Refit;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.FullContact
{
    public class WebhooksModule : NancyModule
    {
        public WebhooksModule(
            IAppSettings appSettings, 
            IPendingCommands pendingCommands, 
            ITinyMessengerHub hub/*, IMailgunWebhooks mailgunWebhooks */)
        {
            Post["/webhooks/fullcontact/person", runAsync: true] = async (_, ct) =>
            {
                // Parse the request data
                var person = this.BindTo(new FullContactPersonResult());
                if (person == null || 
                    person.Result == null)
                {
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse request body.");
                }

                // Get the pending command that corresponds to the posted data
                if (string.IsNullOrWhiteSpace(person.WebhookId))
                {
                    return HttpStatusCode.BadRequest.WithReason("The webhookId property is missing from the request body.");
                }
                var command = pendingCommands.Get(person.WebhookId) as Whois;
                if (command == null)
                {
                    return HttpStatusCode.BadRequest.WithReason("No pending command matching the webhookId could be found.");
                }

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
                new Exception("Requesting message to be sent to Slack.").ToExceptionless().Submit();
                hub.PublishAsync(new TinyMessage<SendMessageToSlack>(slackMessage));
                return HttpStatusCode.OK;
            };

            //Post["/mailgun/{webhookId}", runAsync: true] = async (_, ct) =>
            //{
            //    var webhookId = _.webhookId as string;
            //    if (webhookId.Missing())
            //    {
            //        return HttpStatusCode.NotAcceptable.WithReason("WebhookId is missing.");
            //    }
            //    var webhook = mailgunWebhooks.Get(webhookId);
            //    if (webhook == null)
            //    {
            //        return HttpStatusCode.NotAcceptable.WithReason("The webhook does not exist.");
            //    }

            //    // Prepare message
            //    var slackMessage = new MessageToSlack
            //    {
                    
            //    }

            //    // Post message to Slack
            //    var slackApi = RestService.For<ISlackApi>(appSettings.Get("slack:responseBaseUrl"));
            //    await slackApi.SendMessage(slackMessage, appSettings.Get("slack:responseToken"));
            //    return HttpStatusCode.OK;
            //};
        }
    }
}