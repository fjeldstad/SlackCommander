using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Magnum.StateMachine;
using MassTransit.Saga;
using SlackCommander.Web.SlackMessage.Messages;
using SlackCommander.Web.Whois.Messages;

namespace SlackCommander.Web.Whois
{
    public class WhoisSaga : 
        SagaStateMachine<WhoisSaga>,
        ISaga
    {
        public static State Initial { get; set; }
        public static State Waiting { get; set; }
        public static State Completed { get; set; }

        public static Event<WhoisEmailRequest> WhoisEmailRequestReceived { get; set; }
        public static Event<WhoisTwitterRequest> WhoisTwitterRequestReceived { get; set; }
        public static Event<FullContactPersonResult> ResultReceived { get; set; }

        public MassTransit.IServiceBus Bus { get; set; }
        public Guid CorrelationId { get; set; }
        public string RespondToChannel { get; set; }
        public string Subject { get; set; }

        static WhoisSaga()
        {
            Define(() =>
            {
                Correlate(ResultReceived)
                    .By((saga, message) => saga.CorrelationId.ToString() == message.WebhookId);

                Initially(
                    When(WhoisEmailRequestReceived)
                        .Then((saga, message) => saga.ProcessWhoisRequest(message))
                        .TransitionTo(Waiting),
                    When(WhoisTwitterRequestReceived)
                        .Then((saga, message) => saga.ProcessWhoisRequest(message))
                        .TransitionTo(Waiting));
                    
                During(Waiting,
                    When(ResultReceived)
                        .Then((saga, message) => saga.ProcessWhoisResult(message))
                        .Complete());
            });
        }

        public WhoisSaga(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        void ProcessWhoisRequest(WhoisEmailRequest message)
        {
            RespondToChannel = message.RespondToChannel;
            Subject = message.EmailAddress;
            Bus.Publish(new FullContactEmailLookupRequest
            {
                WebhookId = message.CorrelationId.ToString(),
                EmailAddress = message.EmailAddress
            });
        }

        void ProcessWhoisRequest(WhoisTwitterRequest message)
        {
            RespondToChannel = message.RespondToChannel;
            Subject = message.TwitterHandle;
            Bus.Publish(new FullContactTwitterLookupRequest
            {
                WebhookId = message.CorrelationId.ToString(),
                TwitterHandle = message.TwitterHandle
            });
        }

        void ProcessWhoisResult(FullContactPersonResult message)
        {
            var slackMessage = new MessageToSlack
            {
                channel = RespondToChannel
            };
            if (message.Result.Status != 200 ||
                message.Result.Likelihood < 0.7)
            {
                slackMessage.text = string.Format(
                    "Unfortunately I'm unable to find any reliable information on who *{0}* is. " +
                    "I suggest you try <https://www.google.com/search?q={1}|Google>.",
                    Subject,
                    HttpUtility.UrlEncode(Subject));
            }
            else
            {
                slackMessage.text = string.Format(
                    "I looked up *{0}* and I'm {1:P0} sure this is the person behind it:\n\n",
                    Subject,
                    message.Result.Likelihood);
                slackMessage.text += FormattedSummaryOfWhoisResult(message);
            }

            Bus.Publish(slackMessage);
        }

        private static string FormattedSummaryOfWhoisResult(FullContactPersonResult person)
        {
            var fullName = person.Result.ContactInfo.FullName;
            var location = person.Result.Demographics.LocationGeneral;
            var currentOrganizations = person.Result.Organizations
                .Where(o => o.Current == true)
                .OrderByDescending(o => o.IsPrimary == true);
            var totalFollowers = person.Result.SocialProfiles.Sum(profile => profile.Followers);
            var photo = person.Result.Photos
                .OrderByDescending(p => p.IsPrimary == true)
                .FirstOrDefault();

            var text = new StringBuilder();
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
            if (totalFollowers.HasValue && totalFollowers.Value > 0)
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

            return text.ToString().TrimEnd('\n');
        }
    }
}