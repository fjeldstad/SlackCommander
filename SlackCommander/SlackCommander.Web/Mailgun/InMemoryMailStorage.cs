using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace SlackCommander.Web.Mailgun
{
    public class InMemoryMailStorage : IMailStorage
    {
        public string GetHtmlContents(string id)
        {
            return MemoryCache.Default.Get(id) as string;
        }

        public void Add(string id, string htmlContents)
        {
            MemoryCache.Default.Add(id, htmlContents, DateTimeOffset.Now.AddMinutes(60));
        }
    }
}