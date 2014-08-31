using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NLog;
using Refit;
using SlackCommander.Web.Commands;
using SlackCommander.Web.FullContact;
using TinyMessenger;

namespace SlackCommander.Web.CommandHandlers
{
    public class Whois : SubscriberBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly string _fullContactApiBaseUrl;
        private readonly string _fullContactApiKey;
        private readonly string _fullContactWebhookUrl;
        private readonly IPendingCommands _pendingCommands;

        public Whois(IAppSettings appSettings, IPendingCommands pendingCommands)
        {
            _fullContactApiBaseUrl = appSettings.Get("fullContact:apiBaseUrl");
            _fullContactApiKey = appSettings.Get("fullContact:apiKey");
            _fullContactWebhookUrl = appSettings.Get("fullContact:webhookUrl");
            _pendingCommands = pendingCommands;
        }

        protected async Task<string> InitiateLookup(WhoisEmail command)
        {
            Log.Debug("Initiating whois lookup for e-mail address '{0}'", command.EmailAddress);
            var commandId = Guid.NewGuid().ToString();
            var fullContactApi = RestService.For<IFullContactApi>(_fullContactApiBaseUrl);
            try
            {
                await fullContactApi.LookupByEmail(
                        command.EmailAddress,
                        _fullContactWebhookUrl,
                        commandId,
                        _fullContactApiKey);
                _pendingCommands.Add(commandId, command);
                Log.Debug("Lookup for e-mail address '{0}' is pending.", command.EmailAddress);
                return string.Format("Looking up *{0}*, give me a few moments...", command.EmailAddress);
            }
            catch
            {
                return string.Format("There was a problem with the lookup. I'm sorry.");
            }
        }

        protected async Task<string> InitiateLookup(WhoisTwitter command)
        {
            Log.Debug("Initiating whois lookup for Twitter handle '{0}'", command.TwitterHandle);
            var commandId = Guid.NewGuid().ToString();
            var fullContactApi = RestService.For<IFullContactApi>(_fullContactApiBaseUrl);
            try
            {
                await fullContactApi.LookupByTwitterHandle(
                        command.TwitterHandle,
                        _fullContactWebhookUrl,
                        commandId,
                        _fullContactApiKey);
                _pendingCommands.Add(commandId, command);
                Log.Debug("Lookup for Twitter handle '{0}' is pending.", command.TwitterHandle);
                return string.Format("Looking up *{0}*, give me a few moments...", command.TwitterHandle);
            }
            catch
            {
                return string.Format("There was a problem with the lookup. I'm sorry.");
            }
        }

        protected override IEnumerable<TinyMessageSubscriptionToken> RegisterSubscriptionsCore(ITinyMessengerHub hub)
        {
            yield return hub.Subscribe<TinyMessageWithResponseText<ICommand>>(
                deliveryAction: async message => message.SetResponseText(await InitiateLookup((dynamic)message.Content)),
                messageFilter: message => message.Content is WhoisEmail || message.Content is WhoisTwitter);

            yield return hub.Subscribe<TinyMessage<ICommand>>(
                deliveryAction: async message => await InitiateLookup((dynamic)message.Content),
                messageFilter: message => message.Content is WhoisEmail || message.Content is WhoisTwitter);
        }
    }
}