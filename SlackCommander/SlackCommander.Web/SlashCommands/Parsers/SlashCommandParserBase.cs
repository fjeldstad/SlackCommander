using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.SlashCommands.Parsers
{
    public abstract class SlashCommandParserBase : ISlashCommandParser
    {
        private readonly string _command;

        protected SlashCommandParserBase(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }
            _command = command;
        }

        public Commands.ICommand Parse(SlashCommand slashCommand)
        {
            if (!_command.Equals(slashCommand.command, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(string.Format("The '{0}' command is not supported by this parser (expected '{1}').", slashCommand.command, _command));
            }
            return ParseCore(slashCommand);
        }

        protected abstract Commands.ICommand ParseCore(SlashCommand slashCommand);
    }
}