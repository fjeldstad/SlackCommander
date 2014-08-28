using System;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using Refit;

namespace SlackCommander.Web
{
    public class WebhooksModule : NancyModule
    {
        public WebhooksModule(IAppSettings appSettings, IPendingCommands pendingCommands)
            : base("/webhooks")
        {
            Post["/fullcontact/person", runAsync: true] = async (_, ct) =>
            {
                var personResult = this.Bind<FullContactPersonResult>();
                if (personResult == null)
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason("Unable to parse request body."));
                }
                if (string.IsNullOrWhiteSpace(personResult.WebhookId))
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason("The webhookId property is missing from the request body."));
                }
                var command = pendingCommands.Get(personResult.WebhookId);
                if (command == null)
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason("No pending command matching the webhookId could be found."));
                }
                var slackApi = RestService.For<ISlackApi>(appSettings.Get("slack:responseBaseUrl"));
                await slackApi.SendMessage(new SlackMessage
                {
                    channel = "#random",
                    icon_emoji = ":bust_in_silhouette:",
                    username = "SlackCommander",
                    text = "*Test*"
                }, token: appSettings.Get("slack:responseToken"));
                return await Task.FromResult(HttpStatusCode.OK);
            };
        }
    }
}