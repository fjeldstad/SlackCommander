using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Json;
using Nancy.TinyIoc;
using SlackCommander.Web.CommandHandlers;

namespace SlackCommander.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<Whois>("/whois");
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            
            //StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            //{
            //    return new SlackUserIdentity();
            //}));
        }
    }
}