using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace SlackCommander.Web
{
    public class InMemoryPendingCommands : IPendingCommands
    {
        public void Add(string id, Command command)
        {
            MemoryCache.Default.Set(id, command, DateTimeOffset.Now.AddMinutes(10));
        }

        public Command Get(string id)
        {
            return MemoryCache.Default.Get(id) as Command;
        }
    }
}