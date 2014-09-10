using MassTransit;
using Nancy;
using Nancy.Helpers;
using Nancy.ModelBinding;
using NLog;
using SlackCommander.Web.Messages;

namespace SlackCommander.Web.FullContact
{
    public class FullContactModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public FullContactModule(IServiceBus bus)
        {
            Post["/webhooks/fullcontact/person"] = _ =>
            {
                // Parse the request data
                var person = this.BindTo(new FullContactPersonResult());
                if (person == null || 
                    person.Result == null)
                {
                    Log.Info("Rejected webhook call from FullContact (unable to parse request body).");
                    return HttpStatusCode.BadRequest.WithReason("Unable to parse request body.");
                }

                // Get the pending command that corresponds to the posted data
                if (string.IsNullOrWhiteSpace(person.WebhookId))
                {
                    Log.Info("Rejected a webhook call from FullContact (webhookId missing).");
                    return HttpStatusCode.BadRequest.WithReason("The webhookId property is missing from the request body.");
                }

                bus.Publish(person);
                return HttpStatusCode.OK;
            };
        }
    }
}