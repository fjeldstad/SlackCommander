using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin.Extensions;
using Nancy.TinyIoc;
using Owin;

namespace SlackCommander.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = TinyIoCContainer.Current;
            app.UseNancy(options =>
            {
                options.Bootstrapper = new Bootstrapper(container);
            });
            app.UseStageMarker(PipelineStage.MapHandler);
            app.UseHangfire(config =>
            {
                config.UseTinyIoCActivator(container);
                config.UseSqlServerStorage(container.Resolve<IAppSettings>().Get("azure:sqlServerConnectionString"));
                config.UseServer();
            });
        }
    }
}