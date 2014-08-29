using Nancy;

namespace SlackCommander.Web
{
    public static class HttpStatusCodeExtensions
    {
        public static HttpStatusCodeResponse WithReason(this HttpStatusCode statusCode, string reason)
        {
            return new HttpStatusCodeResponse(statusCode, reason);
        }
    }
}