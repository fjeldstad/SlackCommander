using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackCommander.Web.Mailgun
{
    public interface IMailgunWebhooks
    {
        MailgunWebhook Get(string id);
    }
}
