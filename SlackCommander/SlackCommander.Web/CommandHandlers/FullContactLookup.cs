using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MassTransit;
using NLog;
using Refit;
using SlackCommander.Web.FullContact;
using SlackCommander.Web.Messages;

namespace SlackCommander.Web.CommandHandlers
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
            _fullContactApiBaseUrl = appSettings.Get("fullContact:apiBaseUrl");
            _fullContactApiKey = appSettings.Get("fullContact:apiKey");
            _fullContactWebhookUrl = appSettings.Get("fullContact:webhookUrl");
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