using SlackCommander.Web.Commands;

namespace SlackCommander.Web
{
    public interface IPendingCommands
    {
        void Add(string id, ICommand command);
        ICommand Get(string id);
        void Remove(string id);
    }
}
