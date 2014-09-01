using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.MailChimp
{
    public class DummyMailChimpWebhooks : IMailChimpWebhooks
    {
        private readonly IAppSettings _appSettings;

        public DummyMailChimpWebhooks(IAppSettings appSettings)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException("appSettings");
            }
            _appSettings = appSettings;
        }

        public MailChimpWebhook Get(string id)
        {
            if (!id.Missing() &&
                id.Equals(_appSettings.Get("mailChimp:webhookId"), StringComparison.Ordinal))
            {
                return new MailChimpWebhook
                {
                    Id = id,
                    SlackChannel = _appSettings.Get("mailChimp:webhookPostsToSlackChannel")
                };
            }
            return null;
        }
    }
}