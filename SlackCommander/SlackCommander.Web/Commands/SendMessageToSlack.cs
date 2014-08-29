using System;

namespace SlackCommander.Web.Commands
{
    [Serializable]
    public class SendMessageToSlack : ICommand
    {
        public string Channel { get; set; }
        public string Text { get; set; }
    }
}