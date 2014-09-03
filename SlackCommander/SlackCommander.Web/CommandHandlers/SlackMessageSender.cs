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

        protected async Task Send(MessageToSlack message)
        {
            Log.Debug("Sending message to Slack ({0}: {1})", message.channel, message.text);
            if (message.username.Missing())
            {
                message.username = "SlackCommander";
            }
            if (message.icon_emoji.Missing() && message.icon_url.Missing())
            {
                message.icon_emoji = ":octopus:";
            }
            var slackApi = RestService.For<ISlackApi>(_appSettings.Get("slack:responseBaseUrl"));
            await slackApi.SendMessage(message, _appSettings.Get("slack:responseToken"));
        }

        protected override IEnumerable<TinyMessageSubscriptionToken> RegisterSubscriptionsCore(ITinyMessengerHub hub)
        {
            yield return hub.Subscribe<TinyMessage<MessageToSlack>>(async message => await Send(message.Content));
        }
    }
}