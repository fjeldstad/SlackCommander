using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TinyMessenger;

namespace SlackCommander.Web
{
    public abstract class SubscriberBase : ISubscriber, IDisposable
    {
        private readonly List<TinyMessageSubscriptionToken> _subscriptionTokens = new List<TinyMessageSubscriptionToken>();

        public void RegisterSubscriptions(TinyMessenger.ITinyMessengerHub hub)
        {
            _subscriptionTokens.AddRange(RegisterSubscriptionsCore(hub));
        }

        protected abstract IEnumerable<TinyMessageSubscriptionToken> RegisterSubscriptionsCore(ITinyMessengerHub hub);

        public virtual void Dispose()
        {
            foreach (var subscriptionToken in _subscriptionTokens)
            {
                subscriptionToken.Dispose();
            }
        }
    }
}