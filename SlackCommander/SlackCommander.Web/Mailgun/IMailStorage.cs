using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackCommander.Web.Mailgun
{
    public interface IMailStorage
    {
        string GetHtmlContents(string id);
        void Add(string id, string htmlContents);
    }
}
