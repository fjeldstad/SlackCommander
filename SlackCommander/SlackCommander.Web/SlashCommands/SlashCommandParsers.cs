using System;
using Nancy.TinyIoc;

namespace SlackCommander.Web.SlashCommands
{
    public class SlashCommandParsers
    {
        private readonly TinyIoCContainer _container;

        public SlashCommandParsers(TinyIoCContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            _container = container;
        }

        public ISlashCommandParser For(SlashCommand slashCommand)
        {
            if (!_container.CanResolve<ISlashCommandParser>(slashCommand.command))
            {
                return null;
            }
            return _container.Resolve<ISlashCommandParser>(slashCommand.command);
        }
    }
}