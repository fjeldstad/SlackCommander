using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.SlashCommands
{
    public class SlashCommandsModule : NancyModule
    {
        public SlashCommandsModule(SlashCommandParsers parsers, ITinyMessengerHub hub)
        {
            this.RequiresAuthentication();

            Post["/slashcommands", runAsync: true] = async (_, ct) =>
            {
                var slashCommand = this.Bind<SlashCommand>();
                var parser = parsers.For(slashCommand);
                if (parser == null)
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason(
                        string.Format("The command '{0}' is not supported.", slashCommand.command)));
                }
                try
                {
                    var command = parser.Parse(slashCommand);
                    string responseText = null;
                    await hub.PublishAsyncUsingTask(new TinyMessageWithResponseText<ICommand>(command, s => responseText = s));
                    if (responseText.Missing())
                    {
                        return await Task.FromResult(HttpStatusCode.OK);
                    }
                    return await Task.FromResult(responseText);
                }
                catch (InvalidSlashCommandException ex)
                {
                    return Task.FromResult(HttpStatusCode.BadRequest.WithReason(ex.Message));
                }
            };
        }
    }
}