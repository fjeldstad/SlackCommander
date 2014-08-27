﻿using System;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Json;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using SlackCommander.Web.CommandHandlers;

namespace SlackCommander.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<ICommandHandler, Whois>("/whois");
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            StaticConfiguration.DisableErrorTraces = false;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var appSettings = container.Resolve<IAppSettings>();
                var command = JsonConvert.DeserializeObject<Command>(ctx.Request.Body.AsString());
                if (command == null ||
                    string.IsNullOrWhiteSpace(command.token) ||
                    command.token != appSettings.Get("slack:commandToken"))
                {
                    throw new Exception(string.Format("{0} - {1}", command.token, appSettings.Get("slack:commandToken")));
                    return null;
                }
                return new SlackUserIdentity();
            }));
        }
    }
}