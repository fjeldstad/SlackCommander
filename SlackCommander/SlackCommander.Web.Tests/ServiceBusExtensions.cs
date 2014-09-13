using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magnum.Extensions;
using MassTransit;

namespace SlackCommander.Web.Tests
{
    public static class ServiceBusExtensions
    {
        public static void WaitForStartup(this IServiceBus bus)
        {
            bus.Endpoint.InboundTransport.Receive(c1 => c2 => { }, 1.Milliseconds());
        }
    }
}
