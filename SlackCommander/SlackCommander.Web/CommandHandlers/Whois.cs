using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace SlackCommander.Web.CommandHandlers
{
    public class Whois : ICommandHandler
    {
        public dynamic Handle(Command command)
        {
            try
            {
                new MailAddress(command.text);
            }
            catch
            {
                return string.Format("Sorry, '{0}' does not seem to be a valid e-mail address.", command.text);
            }

            // TODO
            // https://api.fullcontact.com/v2/person.json?email=bart@fullcontact.com&apiKey=xxxx
            return string.Format("Looking up '{0}', just a second...", command.text);
        }
    }
}