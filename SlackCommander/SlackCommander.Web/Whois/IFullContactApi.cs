using System.Threading.Tasks;
using Refit;

namespace SlackCommander.Web.Whois
{
    public interface IFullContactApi
    {
        [Get("/person.json?webhookBody=json")]
        Task LookupByEmail(string email, string webhookUrl, string webhookId, string apiKey);

        [Get("/person.json?webhookBody=json")]
        Task LookupByTwitterHandle([AliasAs("twitter")] string twitterHandle, string webhookUrl, string webhookId, string apiKey);
    }
}
