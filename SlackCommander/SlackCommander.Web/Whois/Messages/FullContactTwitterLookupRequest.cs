using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Whois.Messages
{
    public class FullContactTwitterLookupRequest
    {
        public string WebhookId { get; set; }
        public string TwitterHandle { get; set; }
    }
}