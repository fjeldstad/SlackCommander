using System.Threading.Tasks;
using Refit;
using SlackCommander.Web.SlackMessage.Messages;

namespace SlackCommander.Web
{
    public interface ISlackApi
    {
        [Post("/services/hooks/incoming-webhook")]
        Task SendMessage([Body] MessageToSlack message, string token);
    }
}
