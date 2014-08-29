using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Commands
{
    public abstract class Whois : ICommand
    {
        public string RequestedByUser { get; set; }
        public string RespondToChannel { get; set; }
    }

    public static class WhoisExtensions
    {
        public static string Subject(this Whois command)
        {
            if (command is WhoisEmail)
            {
                return ((WhoisEmail) command).EmailAddress;
            }
            if (command is WhoisTwitter)
            {
                return ((WhoisTwitter)command).TwitterHandle;
            }
            return null;
        }
    }
}