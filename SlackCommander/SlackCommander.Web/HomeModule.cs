using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

namespace SlackCommander.Web
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ =>
            {
                return HttpStatusCode.OK;
            };
        }
    }
}