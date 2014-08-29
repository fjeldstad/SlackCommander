using System;

namespace SlackCommander.Web.SlashCommands
{
    public class InvalidSlashCommandException : Exception
    {
        public InvalidSlashCommandException(string message) : base(message) { }
    }
}