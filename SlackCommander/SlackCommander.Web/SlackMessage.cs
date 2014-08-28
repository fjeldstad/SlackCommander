using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web
{
    public class SlackMessage
    {
        public string username { get; set; }
        public string icon_url { get; set; }
        public string icon_emoji { get; set; }
        public string channel { get; set; }
        public string text { get; set; }
    }
}