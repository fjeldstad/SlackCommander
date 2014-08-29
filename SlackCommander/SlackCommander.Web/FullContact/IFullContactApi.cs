using System.Threading.Tasks;
using Refit;

namespace SlackCommander.Web.FullContact
{
    public interface IFullContactApi
    {
        [Get("/person.json?webhookBody=json")]
        Task LookupByEmail(string email, string webhookUrl, [AliasAs("webhookId")] string commandId, string apiKey);

        [Get("/person.json?webhookBody=json")]
        Task LookupByTwitterHandle([AliasAs("twitter")] string twitterHandle, string webhookUrl, [AliasAs("webhookId")] string commandId, string apiKey);
    }
}
