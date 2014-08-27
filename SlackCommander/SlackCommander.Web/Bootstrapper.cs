using System;
using System.Text;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Helpers;
using Nancy.Json;
using Nancy.TinyIoc;
using SlackCommander.Web.CommandHandlers;

namespace SlackCommander.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly TinyIoCContainer _container;

        public Bootstrapper(TinyIoCContainer container)
        {
            _container = container;
        }

        protected override TinyIoCContainer GetApplicationContainer()
        {
            return _container;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<ICommandHandler, Whois>("/whois");
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var appSettings = container.Resolve<IAppSettings>();
                var body = HttpUtility.ParseQueryString(ctx.Request.Body.AsString(), Encoding.UTF8);
                if (body == null ||
                    string.IsNullOrWhiteSpace(body["token"]) ||
                    body["token"] != appSettings.Get("slack:commandToken"))
                {
                    return null;
                }
                return new SlackUserIdentity();
            }));
        }
    }
}