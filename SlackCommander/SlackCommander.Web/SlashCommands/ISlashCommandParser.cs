using SlackCommander.Web.Commands;

namespace SlackCommander.Web.SlashCommands
{
    public interface ISlashCommandParser
    {
        ICommand Parse(SlashCommand slashCommand);
    }
}
