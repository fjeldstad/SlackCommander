using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.CommandHandlers
{
    public class Whois : ICommandHandler
    {
        public dynamic Handle(Command command)
        {
            return "Hello " + command.user_name;
        }
    }
}