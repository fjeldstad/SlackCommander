using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Refit;

namespace SlackCommander.Web
{
    public interface ISlackApi
    {
        [Post("/services/hooks/incoming-webhook")]
        Task SendMessage([Body] SlackMessage message, string token);
    }
}
