using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using Refit;

namespace SlackCommander.Web
{
    public class WebhooksModule : NancyModule
    {
        public WebhooksModule(IAppSettings appSettings, IPendingCommands pendingCommands)
            : base("/webhooks")
        {
            Post["/fullcontact/person", runAsync: true] = async (_, ct) =>
            {
                // Parse the request data
                var person = this.BindTo(new FullContactPersonResult());
                if (person == null || 
                    person.Result == null)
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason("Unable to parse request body."));
                }

                // Get the pending command that corresponds to the posted data
                if (string.IsNullOrWhiteSpace(person.WebhookId))
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason("The webhookId property is missing from the request body."));
                }
                var command = pendingCommands.Get(person.WebhookId);
                if (command == null)
                {
                    return await Task.FromResult(HttpStatusCode.BadRequest.WithReason("No pending command matching the webhookId could be found."));
                }

                // Prepare message
                var slackMessage = new SlackMessage
                {
                    username = "SlackCommander",
                    icon_emoji = ":bust_in_silhouette:",
                    channel = "@" + command.user_name
                };
                if (person.Result.Status != 200 ||
                    person.Result.Likelihood < 0.7)
                {
                    slackMessage.text = string.Format(
                        "Unfortunately I'm unable to find any reliable information on who *{0}* is. " +
                        "I suggest you try <https://google.com/q={0}|Google>.",
                        command.text);
                }
                else
                {
                    var fullName = person.Result.ContactInfo.FullName;
                    var location = person.Result.Demographics.LocationGeneral;
                    var currentOrganizations = person.Result.Organizations
                        .Where(o => string.IsNullOrWhiteSpace(o.EndDate) || o.Current == true)
                        .OrderByDescending(o => o.IsPrimary == true);
                    var totalFollowers = person.Result.SocialProfiles.Sum(profile => profile.Followers);
                    var photo = person.Result.Photos
                        .OrderByDescending(p => p.IsPrimary == true)
                        .FirstOrDefault();

                    var text = new StringBuilder();
                    text.AppendFormat(
                        "I looked up *{0}* and I'm {1:P0} sure this is the person behind it:\n\n", 
                        command.text, 
                        person.Result.Likelihood);

                    if (!fullName.Missing())
                    {
                        text.AppendFormat("*{0}*", fullName);
                        if (!location.Missing())
                        {
                            text.AppendFormat(" _{0}_", location);
                        }
                        text.Append("\n");
                    }
                    foreach (var organization in currentOrganizations)
                    {
                        if (organization != null &&
                            !organization.Description.Missing())
                        {
                            text.AppendFormat("{0}\n", organization.Description);
                        }
                    }
                    if (totalFollowers.HasValue)
                    {
                        text.AppendFormat("{0} followers on social media", totalFollowers.Value);
                        if (totalFollowers > 1000)
                        {
                            text.Append(" (wow!)");
                        }
                        text.Append("\n");
                    }
                    if (photo != null)
                    {
                        text.AppendFormat("<{0}|Profile photo>\n", photo.Url);
                    }

                    slackMessage.text = text.ToString().TrimEnd('\n');
                }

                // Post message to Slack
                var slackApi = RestService.For<ISlackApi>(appSettings.Get("slack:responseBaseUrl"));
                await slackApi.SendMessage(slackMessage, appSettings.Get("slack:responseToken"));
                return await Task.FromResult(HttpStatusCode.OK);
            };
        }
    }
}