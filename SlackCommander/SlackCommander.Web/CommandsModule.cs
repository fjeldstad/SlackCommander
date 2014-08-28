using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;

namespace SlackCommander.Web
{
    public class CommandsModule : NancyModule
    {
        public CommandsModule(CommandHandler commandHandler)
        {
            this.RequiresAuthentication();

            Post["/commands", runAsync: true] = async (_, ct) =>
            {
                var command = this.Bind<Command>();
                return await commandHandler.Handle(command);
            };
        }
    }
}