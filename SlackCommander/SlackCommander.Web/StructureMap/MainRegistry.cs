using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MassTransit;
using MassTransit.Saga;
using SlackCommander.Web.MailChimp;
using SlackCommander.Web.Mailgun;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace SlackCommander.Web.StructureMap
{
    public class MainRegistry : Registry
    {
        public MainRegistry()
        {
            For(typeof(ISagaRepository<>)).Use(typeof(InMemorySagaRepository<>)).Singleton();
            Scan(config =>
            {
                config.TheCallingAssembly();
                config.AssemblyContainingType<Bootstrapper>();
                config.AddAllTypesOf<IAppSettings>();
                config.AddAllTypesOf<IMailChimpWebhooks>();
                config.AddAllTypesOf<IMailgunWebhooks>();
                config.AddAllTypesOf<IConsumer>();
                config.AddAllTypesOf<ISaga>();
            });
        }
    }
}