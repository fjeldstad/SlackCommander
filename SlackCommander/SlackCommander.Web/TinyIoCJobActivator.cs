using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire;
using Nancy.TinyIoc;

namespace SlackCommander.Web
{
    public class TinyIoCJobActivator : JobActivator
    {
        private readonly TinyIoCContainer _container;

        public TinyIoCJobActivator(TinyIoCContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            _container = container;
        }

        public override object ActivateJob(Type jobType)
        {
            return _container.Resolve(jobType);
        }
    }

    public static class TinyIoCBootstrapperConfigurationExtensions
    {
        public static void UseTinyIoCActivator(this IBootstrapperConfiguration configuration, TinyIoCContainer container)
        {
            configuration.UseActivator(new TinyIoCJobActivator(container));
        }
    }
}