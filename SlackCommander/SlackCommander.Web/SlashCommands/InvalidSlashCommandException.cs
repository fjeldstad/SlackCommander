using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.SlashCommands
{
    public class InvalidSlashCommandException : Exception
    {
        public InvalidSlashCommandException(string message) : base(message) { }
    }
}