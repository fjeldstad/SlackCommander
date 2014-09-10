using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Messages
{
    [Serializable]
    public class SlashCommandResponse
    {
        public string Text { get; set; }
    }
}