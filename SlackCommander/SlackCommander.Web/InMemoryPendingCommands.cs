using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using SlackCommander.Web.Commands;

namespace SlackCommander.Web
{
    public class InMemoryPendingCommands : IPendingCommands
    {
        public void Add(string id, ICommand command)
        {
            MemoryCache.Default.Set(id, command, DateTimeOffset.Now.AddMinutes(10));
        }

        public ICommand Get(string id)
        {
            return MemoryCache.Default.Get(id) as ICommand;
        }

        public void Remove(string id)
        {
            MemoryCache.Default.Remove(id);
        }
    }
}