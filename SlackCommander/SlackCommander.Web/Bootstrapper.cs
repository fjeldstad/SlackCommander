using System.Text;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Helpers;
using Nancy.TinyIoc;
using SlackCommander.Web.SlashCommands;
using TinyMessenger;
using Exceptionless;

namespace SlackCommander.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            // Slash command parsers
            container.Register<ISlashCommandParser, SlashCommands.Parsers.Whois>(SlashCommands.Parsers.Whois.Command);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // Log exceptions to Exceptionless
            ExceptionlessClient.Current.RegisterNancy(pipelines);

            // Register subscriptions
            var hub = container.Resolve<ITinyMessengerHub>();
            foreach (var subscriber in container.ResolveAll<ISubscriber>(includeUnnamed: false))
            {
                subscriber.RegisterSubscriptions(hub);
            }
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
}