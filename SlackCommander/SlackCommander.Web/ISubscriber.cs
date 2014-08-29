using TinyMessenger;

namespace SlackCommander.Web
{
    public interface ISubscriber
    {
        void RegisterSubscriptions(ITinyMessengerHub hub);
    }
}
