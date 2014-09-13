using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Nancy;
using Nancy.Security;
using Nancy.Testing;

namespace SlackCommander.Web.Tests
{
    public static class NancyTestingExtensions
    {
        public static void WithAuthenticatedUser(this NancyContext nancyContext)
        {
            var user = new Mock<IUserIdentity>();
            user.SetupGet(x => x.UserName).Returns("test");
            nancyContext.CurrentUser = user.Object;
        }
    }
}
