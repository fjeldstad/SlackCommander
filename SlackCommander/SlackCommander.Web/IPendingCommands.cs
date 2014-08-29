using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
