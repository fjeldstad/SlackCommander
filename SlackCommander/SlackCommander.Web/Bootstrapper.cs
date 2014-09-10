using System.Text;
using MassTransit;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.StructureMap;
using Nancy.Extensions;
using Nancy.Helpers;
using NLog;
using StructureMap;
using StructureMap.Graph;

namespace SlackCommander.Web
{
    public class Bootstrapper : StructureMapNancyBootstrapper
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override void ConfigureApplicationContainer(IContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Configure(config => config.Scan(cfg =>
            {
                cfg.TheCallingAssembly();
                cfg.LookForRegistries();
            }));
        }

        protected override void ApplicationStartup(IContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            var bus = ServiceBusFactory.New(config =>
            {
                config.ReceiveFrom("loopback://localhost/queue");
                config.Subscribe(x =>
                {
                    x.LoadFrom(container);
                });
            });
            container.Inject(bus);

            // Log unhandled exceptions
            pipelines.OnError += (ctx, ex) =>
            {
                Log.Error("Unhandled exception.", ex);
                return null;
            };
        }

        protected override void RequestStartup(IContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            
            // Enable token authentication for incoming slash commands
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var appSettings = container.GetInstance<IAppSettings>();
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
}