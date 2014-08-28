using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

namespace SlackCommander.Web
{
    public class HttpStatusCodeResponse : Response
    {
        public HttpStatusCodeResponse(HttpStatusCode statusCode, string reason = null)
        {
            StatusCode = statusCode;
            ReasonPhrase = reason;
        }
    }
}