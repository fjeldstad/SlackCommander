using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlackCommander.Web.MailChimp;

namespace SlackCommander.Web.MailChimp
{
    public interface IMailChimpWebhooks
    {
        MailChimpWebhook Get(string id);
    }
}
