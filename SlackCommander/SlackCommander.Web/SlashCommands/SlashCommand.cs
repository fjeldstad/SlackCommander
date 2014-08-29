﻿using System;

namespace SlackCommander.Web.SlashCommands
{
    [Serializable]
    public class SlashCommand
    {
        public string team_id { get; set; }
        public string channel_id { get; set; }
        public string channel_name { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string command { get; set; }
        public string text { get; set; }
    }
}