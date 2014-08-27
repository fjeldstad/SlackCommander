using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web
{
    public class Command
    {
        public string team_id { get; set; }
        public string channel_id { get; set; }
        public string channel_name { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string command { get; set; }
        public string text { get; set; }
    }

    public static class CommandExtensions
    {
        public static bool IsValid(this Command command)
        {
            return command != null &&
                   !string.IsNullOrWhiteSpace(command.command) &&
                   !string.IsNullOrWhiteSpace(command.text);
        }
    }
}