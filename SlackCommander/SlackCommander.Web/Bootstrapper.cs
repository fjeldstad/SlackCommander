using System;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Helpers;
using Nancy.TinyIoc;
using NLog;
using SlackCommander.Web.Commands;
using SlackCommander.Web.SlashCommands;
using TinyMessenger;

namespace SlackCommander.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            // Slash command parsers
            container.Register<ISlashCommandParser, SlashCommands.Parsers.Whois>(SlashCommands.Parsers.Whois.Command);

            container.Register<IPendingCommands>(new InMemoryPendingCommands());
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // Register subscriptions
            var hub = container.Resolve<ITinyMessengerHub>();
            foreach (var subscriber in container.ResolveAll<ISubscriber>(includeUnnamed: false))
            {
                subscriber.RegisterSubscriptions(hub);
            }

            // Log unhandled exceptions
            pipelines.OnError += (ctx, ex) =>
            {
                Log.Error("Unhandled exception.", ex);
                return null;
            };
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            
            // Enable token authentication for incoming slash commands
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var appSettings = container.Resolve<IAppSettings>();
                var body = HttpUtility.ParseQueryString(ctx.Request.Body.AsString(), Encoding.UTF8);
                if (body == null ||
                    string.IsNullOrWhiteSpace(body["token"]) ||
                    body["token"] != appSettings.Get("slack:slashCommandToken"))
                {
                    return null;
                }
                return new SlackUserIdentity();
            }));
        }
    }

    public class TempModule : NancyModule
    {
        public TempModule()
        {
            Get["/temp"] = _ => {
                                     throw new Exception("Temp!");
            };
        }
    }
}