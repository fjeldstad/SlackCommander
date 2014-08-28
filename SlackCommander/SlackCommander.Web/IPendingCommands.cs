using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackCommander.Web
{
    public interface IPendingCommands
    {
        void Add(string id, Command command);
        Command Get(string id);
    }
}
