using System;

namespace SlackCommander.Web.Commands
{
    [Serializable]
    public class WhoisTwitter : Whois
    {
        public string TwitterHandle { get; set; }
    }
}