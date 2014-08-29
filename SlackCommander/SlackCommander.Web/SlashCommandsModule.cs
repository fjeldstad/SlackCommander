using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using TinyMessenger;

namespace SlackCommander.Web
{
    public class SlashCommandsModule : NancyModule
    {
        public SlashCommandsModule(SlashCommandHandler commandHandler, ITinyMessengerHub hub)
        {
            this.RequiresAuthentication();

            Post["/commands", runAsync: true] = async (_, ct) =>
            {
                var command = this.Bind<SlashCommand>();
                
                return await commandHandler.Handle(command);
            };
        }
    }
}