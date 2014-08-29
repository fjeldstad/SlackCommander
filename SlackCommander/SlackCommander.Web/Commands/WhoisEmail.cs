using System;

namespace SlackCommander.Web.Commands
{
    [Serializable]
    public class WhoisEmail : Whois
    {
        public string EmailAddress { get; set; }
    }
}