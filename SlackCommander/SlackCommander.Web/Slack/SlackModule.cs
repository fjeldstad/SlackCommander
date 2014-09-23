using System;
using System.Threading.Tasks;
using MassTransit;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NLog;
using SlackCommander.Web.Messages;


namespace SlackCommander.Web.Slack
{
    public class SlackModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public SlackModule(IServiceBus bus)
        {
            Post["/slashcommands", runAsync: true] = async (_, ct) =>
            {
                var slashCommand = this.Bind<SlashCommand>();
                if (slashCommand == null)
                {
                    Log.Info("Rejected an incoming slash command (unable to parse request body).");
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse command.");
                }

                var tcs = new TaskCompletionSource<string>();
                bus.PublishRequest(slashCommand, callback =>
                {
                    callback.Handle<SlashCommandResponse>(response => tcs.SetResult(response.Text));
                    callback.SetTimeout(TimeSpan.FromSeconds(10));
                });
                var responseText = await tcs.Task;
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    return HttpStatusCode.OK;
                }
                return responseText;
            };
        }
    }
}