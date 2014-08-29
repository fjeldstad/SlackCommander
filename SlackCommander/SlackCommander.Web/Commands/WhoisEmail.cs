using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Commands
{
    [Serializable]
    public class WhoisEmail : Whois
    {
        public string EmailAddress { get; set; }
    }
}