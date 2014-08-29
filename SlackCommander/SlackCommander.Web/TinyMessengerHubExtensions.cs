using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TinyMessenger;

namespace SlackCommander.Web
{
    public static class TinyMessengerHubExtensions
    {
        public static Task PublishAsyncUsingTask<TMessage>(this ITinyMessengerHub hub, TMessage message)
            where TMessage : class, ITinyMessage
        {
            var taskCompletitionSource = new TaskCompletionSource<object>();
            hub.PublishAsync(message, callback => taskCompletitionSource.SetResult(null));
            return taskCompletitionSource.Task;
        }
    }
}