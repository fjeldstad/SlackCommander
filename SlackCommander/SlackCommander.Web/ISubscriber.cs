using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyMessenger;

namespace SlackCommander.Web
{
    public interface ISubscriber
    {
        void RegisterSubscriptions(ITinyMessengerHub hub);
    }
}
