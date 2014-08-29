using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlackCommander.Web.Commands;
using SlackCommander.Web.SlashCommands;

namespace SlackCommander.Web.SlashCommands
{
    public interface ISlashCommandParser
    {
        ICommand Parse(SlashCommand slashCommand);
    }
}
