using System;
using MassTransit;
using NLog;
using Refit;
using SlackCommander.Web.Whois.Messages;

namespace SlackCommander.Web.Whois
{
    public class FullContactLookup : 
        Consumes<FullContactEmailLookupRequest>.All,
        Consumes<FullContactTwitterLookupRequest>.All
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly string _fullContactApiBaseUrl;
        private readonly string _fullContactApiKey;
        private readonly string _fullContactWebhookUrl;

        public FullContactLookup(IAppSettings appSettings)
        {
            _fullContactApiBaseUrl = appSettings.Get("whois:fullContactApiBaseUrl");
            _fullContactApiKey = appSettings.Get("whois:fullContactApiKey");
            _fullContactWebhookUrl = appSettings.Get("whois:fullContactWebhookUrl");
        }

        public void Consume(FullContactEmailLookupRequest message)
        {
            Log.Debug("Initiating FullContact e-mail lookup for '{0}'", message.EmailAddress);
            try
            {
                var fullContactApi = RestService.For<IFullContactApi>(_fullContactApiBaseUrl);
                fullContactApi.LookupByEmail(
                    message.EmailAddress,
                    _fullContactWebhookUrl,
                    message.WebhookId,
                    _fullContactApiKey).Wait(TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                Log.Error("FullContact e-mail lookup failed.", ex);
            }
        }

        public void Consume(FullContactTwitterLookupRequest message)
        {
            Log.Debug("Initiating FullContact Twitter lookup for '{0}'", message.TwitterHandle);
            try
            {
                var fullContactApi = RestService.For<IFullContactApi>(_fullContactApiBaseUrl);
                fullContactApi.LookupByTwitterHandle(
                    message.TwitterHandle,
                    _fullContactWebhookUrl,
                    message.WebhookId,
                    _fullContactApiKey).Wait(TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                Log.Error("FullContact Twitter lookup failed.", ex);
            }
        }
    }
}