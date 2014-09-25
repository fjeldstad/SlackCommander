using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MassTransit;
using NLog;
using Refit;
using SlackCommander.Web.SlackMessage.Messages;

namespace SlackCommander.Web.SlackMessage
{
    public class SlackMessageSender : Consumes<MessageToSlack>.All
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IAppSettings _appSettings;

        public SlackMessageSender(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void Consume(MessageToSlack message)
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
            var slackApi = RestService.For<ISlackApi>(_appSettings.Get("slackMessage:responseBaseUrl"));
            slackApi.SendMessage(message, _appSettings.Get("slackMessage:responseToken"));
        }
    }
}