using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web
{
    public class TinyMessageWithResponseText<TContent> : ITinyMessage
    {
        public object Sender { get { return null; } }
        public Action<string> SetResponseText { get; protected set; }
        public TContent Content { get; protected set; }

        public TinyMessageWithResponseText(TContent content, Action<string> setResponseTextAction)
        {
            if (setResponseTextAction == null)
            {
                throw new ArgumentNullException("setResponseTextAction");
            }

            SetResponseText = setResponseTextAction;
            Content = content;
        }
    }
}