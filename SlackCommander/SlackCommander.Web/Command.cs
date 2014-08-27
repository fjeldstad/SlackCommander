using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.TinyIoc;

namespace SlackCommander.Web
{
    public class Command
    {
        public string team_id { get; set; }
        public string channel_id { get; set; }
        public string channel_name { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string command { get; set; }
        public string text { get; set; }
    }

    public static class CommandExtensions
    {
        private static bool IsValid(this Command command)
        {
            return command != null &&
                   !string.IsNullOrWhiteSpace(command.command) &&
                   !string.IsNullOrWhiteSpace(command.text);
        }

        public static dynamic Handle(this Command command)
        {
            if (!IsValid(command))
            {
                return HttpStatusCode.BadRequest;
            }
            var handler = TinyIoCContainer.Current.Resolve<ICommandHandler>(command.command);
            if (handler == null)
            {
                return HttpStatusCode.BadRequest;
            }
            return handler.Handle(command);
        }
    }
}