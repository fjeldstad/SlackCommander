using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MassTransit;

namespace SlackCommander.Web.Whois.Messages
{
    public class FullContactEmailLookupRequest
    {
        public string WebhookId { get; set; }
        public string EmailAddress { get; set; }
    }
}