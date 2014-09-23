using System.Linq;
using System.Text;
using Autofac;
using Magnum.Extensions;
using MassTransit;
using MassTransit.Saga;
using Nancy;
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
            builder.RegisterType<AzureTableStorageTodoService>().As<ITodoService>().SingleInstance();

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
    }
}