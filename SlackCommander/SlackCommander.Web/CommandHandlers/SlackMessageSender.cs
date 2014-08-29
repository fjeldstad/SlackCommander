using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exceptionless;
using Refit;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.CommandHandlers
{
    public class SlackMessageSender : SubscriberBase
    {
        private readonly IAppSettings _appSettings;

        public SlackMessageSender(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected void Send(SendMessageToSlack message)
        {
            new Exception("Sending message to Slack.").ToExceptionless().Submit();
            var slackApi = RestService.For<ISlackApi>(_appSettings.Get("slack:responseBaseUrl"));
            slackApi.SendMessage(
                new MessageToSlack
                {
                    username = "SlackCommander",
                    icon_emoji = ":octopus:",
                    channel = message.Channel,
                    text = message.Text
                },
                _appSettings.Get("slack:responseToken"));
        }

        protected override IEnumerable<TinyMessageSubscriptionToken> RegisterSubscriptionsCore(ITinyMessengerHub hub)
        {
            new Exception("Subscribing to TinyMessage<SendMessageToSlack>.").ToExceptionless().Submit();
            yield return hub.Subscribe<TinyMessage<SendMessageToSlack>>(message => Send(message.Content));
        }
    }
}