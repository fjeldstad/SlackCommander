using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;

namespace SlackCommander.Web
{
    public class CommandsModule : NancyModule
    {
        public CommandsModule(CommandHandler commandHandler)
        {
            //this.RequiresAuthentication();

            Post["/commands"] = _ =>
            {
                var command = this.Bind<Command>();
                return commandHandler.Handle(command);
            };
        }
    }
}