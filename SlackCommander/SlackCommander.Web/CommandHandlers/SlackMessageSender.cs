using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Refit;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.CommandHandlers
{
    public class SlackMessageSender : SubscriberBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IAppSettings _appSettings;

        public SlackMessageSender(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected async Task Send(SendMessageToSlack message)
        {
            Log.Debug("Sending message to Slack ({0}: {1})", message.Channel, message.Text);
            var slackApi = RestService.For<ISlackApi>(_appSettings.Get("slack:responseBaseUrl"));
            await slackApi.SendMessage(
                new MessageToSlack
                {
                    username = "SlackCommander",
                    icon_emoji = ":octopus:",
                    channel = message.Channel,
                    text = message.Text,
                    unfurl_links = message.UnfurlLinks
                },
                _appSettings.Get("slack:responseToken"));
        }

        protected override IEnumerable<TinyMessageSubscriptionToken> RegisterSubscriptionsCore(ITinyMessengerHub hub)
        {
            yield return hub.Subscribe<TinyMessage<SendMessageToSlack>>(async message => await Send(message.Content));
        }
    }
}