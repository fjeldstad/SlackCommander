using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Commands
{
    [Serializable]
    public class WhoisTwitter : Whois
    {
        public string TwitterHandle { get; set; }
    }
}