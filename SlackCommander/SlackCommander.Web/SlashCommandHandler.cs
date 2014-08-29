using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Nancy;
using Nancy.TinyIoc;

namespace SlackCommander.Web
{
    public class SlashCommandHandler
    {
        private readonly TinyIoCContainer _container;

        public SlashCommandHandler(TinyIoCContainer container)
        {
            _container = container;
        }

        public Task<dynamic> Handle(SlashCommand command)
        {
            if (!_container.CanResolve<ISlashCommandHandler>(command.command))
            {
                return Task.FromResult((dynamic)HttpStatusCode.BadRequest);
            }
            return _container.Resolve<ISlashCommandHandler>(command.command).Handle(command);
        }
    }
}