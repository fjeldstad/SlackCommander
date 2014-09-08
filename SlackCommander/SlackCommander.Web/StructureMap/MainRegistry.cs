using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackCommander.Web.Commands;
using SlackCommander.Web.MailChimp;
using SlackCommander.Web.Mailgun;
using SlackCommander.Web.SlashCommands;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using TinyMessenger;

namespace SlackCommander.Web.StructureMap
{
    public class MainRegistry : Registry
    {
        public MainRegistry()
        {
            Scan(config =>
            {
                config.TheCallingAssembly();
                config.AssemblyContainingType<Bootstrapper>();
                config.AddAllTypesOf<IAppSettings>();
                config.AddAllTypesOf<ITinyMessengerHub>();
                config.AddAllTypesOf<ISubscriber>();
                config.AddAllTypesOf<IMailChimpWebhooks>();
                config.AddAllTypesOf<IMailgunWebhooks>();
                config.AddAllTypesOf<ISlashCommandParser>();
                config.AddAllTypesOf<IPendingCommands>();
            });
        }
    }
}