using System;
using MassTransit;

namespace SlackCommander.Web.Whois.Messages
{
    [Serializable]
    public class WhoisEmailRequest : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string RequestedByUser { get; set; }
        public string RespondToChannel { get; set; }
        public string EmailAddress { get; set; }
    }
}