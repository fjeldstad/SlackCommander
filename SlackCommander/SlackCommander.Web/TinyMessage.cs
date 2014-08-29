using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web
{
    public class TinyMessage<TContent> : ITinyMessage
    {
        public object Sender { get { return null; } }
        public TContent Content { get; protected set; }

        public TinyMessage(TContent content)
        {
            Content = content;
        }
    }
}