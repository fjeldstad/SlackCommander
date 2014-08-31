using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NLog;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.SlashCommands
{
    public class SlashCommandsModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public SlashCommandsModule(SlashCommandParsers parsers, ITinyMessengerHub hub)
        {
            this.RequiresAuthentication();

            Post["/slashcommands", runAsync: true] = async (_, ct) =>
            {
                var slashCommand = this.Bind<SlashCommand>();
                if (slashCommand == null)
                {
                    Log.Info("Rejected an incoming slash command (unable to parse request body).");
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse command.");
                }
                var parser = parsers.For(slashCommand);
                if (parser == null)
                {
                    Log.Info("Rejected an incoming slash command ('{0}' is not supported).", slashCommand.command);
                    return HttpStatusCode.BadRequest.WithReason(
                        string.Format("The command '{0}' is not supported.", slashCommand.command));
                }
                try
                {
                    var command = parser.Parse(slashCommand);
                    if (command == null)
                    {
                        Log.Info("Rejected an incoming slash command (could not parse command).");
                        return HttpStatusCode.InternalServerError.WithReason(
                                "Command is supported but could not be parsed.");
                    }

                    Log.Debug("Publishing command as a result of parsing incoming '{0}' slash command.",
                        slashCommand.command);

                    string responseText = null;
                    await hub.PublishAsyncUsingTask(new TinyMessageWithResponseText<ICommand>(command, s => responseText = s));
                    if (responseText.Missing())
                    {
                        return HttpStatusCode.OK;
                    }
                    return responseText;
                }
                catch (InvalidSlashCommandException ex)
                {
                    return ex.Message;
                }
            };
        }
    }
}