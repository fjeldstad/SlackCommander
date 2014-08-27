using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.TinyIoc;

namespace SlackCommander.Web
{
    public class CommandHandler
    {
        private readonly TinyIoCContainer _container;

        public CommandHandler(TinyIoCContainer container)
        {
            _container = container;
        }

        public dynamic Handle(Command command)
        {
            if (!_container.CanResolve<ICommandHandler>(command.command))
            {
                return HttpStatusCode.BadRequest;
            }
            return _container.Resolve<ICommandHandler>(command.command).Handle(command);
        }
    }
}