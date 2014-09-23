using System.Linq;
using System.Text;
using Autofac;
using Magnum.Extensions;
using MassTransit;
using MassTransit.Saga;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Extensions;
using Nancy.Helpers;
using NLog;
using SlackCommander.Web.MailChimp;
using SlackCommander.Web.Mailgun;
using SlackCommander.Web.Todo;

namespace SlackCommander.Web
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            base.ConfigureApplicationContainer(container);

            var builder = new ContainerBuilder();

            builder.RegisterType<AppSettings>().As<IAppSettings>().SingleInstance();
            builder.RegisterType<DummyMailChimpWebhooks>().As<IMailChimpWebhooks>().SingleInstance();
            builder.RegisterType<DummyMailgunWebhooks>().As<IMailgunWebhooks>().SingleInstance();
            builder.RegisterType<DummyTodoService>().As<ITodoService>().SingleInstance();

            // Consumers and sagas
            builder.RegisterAssemblyTypes(typeof(Bootstrapper).Assembly)
                .Where(t => t.Implements<ISaga>() ||
                            t.Implements<IConsumer>())
                .AsSelf();

            // Saga repositories
            builder.RegisterGeneric(typeof(InMemorySagaRepository<>))
                .As(typeof(ISagaRepository<>))
                .SingleInstance();

            // Service bus
            builder.Register(c => ServiceBusFactory.New(sbc =>
            {
                sbc.ReceiveFrom("loopback://localhost/queue");
                sbc.Subscribe(x => x.LoadFrom(container));
            })).As<IServiceBus>().SingleInstance();

            builder.Update(container.ComponentRegistry);
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // Log unhandled exceptions
            pipelines.OnError += (ctx, ex) =>
            {
                Log.Error("Unhandled exception.", ex);
                return null;
            };
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
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
}