using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Mailgun
{
    public class DummyMailgunWebhooks : IMailgunWebhooks
    {
        public MailgunWebhook Get(string id)
        {
            return new MailgunWebhook
            {
                Id = id,
                SlackChannel = "@slackbot"
            };
        }

        public void Add(MailgunWebhook webhook)
        {

        }
    }
}