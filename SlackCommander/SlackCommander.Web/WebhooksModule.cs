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
                // TODO Deserialize JSON body and extract person info
                var slackApi = RestService.For<ISlackApi>(appSettings.Get("slack:responseBaseUrl"));
                await slackApi.SendMessage(new SlackMessage
                {
                    channel = "#random",
                    icon_emoji = ":mag_right:",
                    username = "SlackCommander",
                    text = "*Test*"
                }, token: appSettings.Get("slack:responseToken"));
                return await Task.FromResult(HttpStatusCode.OK);
            };
        }
    }
}