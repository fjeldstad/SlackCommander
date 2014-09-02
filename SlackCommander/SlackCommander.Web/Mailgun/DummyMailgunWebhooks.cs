using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Mailgun
{
    public class DummyMailgunWebhooks : IMailgunWebhooks
    {
        private readonly IAppSettings _appSettings;

        public DummyMailgunWebhooks(IAppSettings appSettings)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException("appSettings");
            }
            _appSettings = appSettings;
        }

        public MailgunWebhook Get(string id)
        {
            if (!id.Missing() &&
                id.Equals(_appSettings.Get("mailgun:webhookId"), StringComparison.Ordinal))
            {
                return new MailgunWebhook
                {
                    Id = id
                };
            }
            return null;
        }
    }
}