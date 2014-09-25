using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magnum.Extensions;
using MassTransit;
using MassTransit.BusConfigurators;
using MassTransit.RequestResponse.Configurators;
using MassTransit.TestFramework;
using MassTransit.TestFramework.Fixtures;
using MassTransit.Transports.Loopback;
using Moq;
using Nancy;
using Nancy.Security;
using Nancy.Testing;
using NUnit.Framework;

namespace SlackCommander.Web.Tests
{
    public class SlackModuleTests : LocalTestFixture<LoopbackTransportFactory>
    {
        //public SlackModuleTests()
        //{
        //    LocalUri = new Uri("loopback://localhost/test");
        //}

        //[Test]
        //[Ignore("Modularization means that slash commands will not be published on the bus anymore.")]
        //public void ValidSlashCommandReceived_SlashCommandPublishedOnBus()
        //{
        //    var slashCommandHandler = new TestConsumerOf<SlashCommand>(m => 
        //        LocalBus.Context().Respond(new SlashCommandResponse { Text = "OK" }));
        //    LocalBus.SubscribeConsumer(() => slashCommandHandler);
        //    LocalBus.WaitForStartup();
            
        //    var msg = new SlashCommand
        //    {
        //        team_id = "team_id",
        //        channel_id = "channel_id",
        //        channel_name = "channel_name",
        //        user_id = "user_id",
        //        user_name = "user_name",
        //        command = "command",
        //        text = "text"
        //    };
        //    var browser = new Browser(with =>
        //    {
        //        with.Module<SlackModule>();
        //        with.Dependency<IServiceBus>((object)LocalBus);
        //        with.RequestStartup((container, pipelines, nancyContext) => nancyContext.WithAuthenticatedUser());
        //    });

        //    browser.Post("/slashcommands", with =>
        //    {
        //        with.FormValue("team_id", msg.team_id);
        //        with.FormValue("channel_id", msg.channel_id);
        //        with.FormValue("channel_name", msg.channel_name);
        //        with.FormValue("user_id", msg.user_id);
        //        with.FormValue("user_name", msg.user_name);
        //        with.FormValue("command", msg.command);
        //        with.FormValue("text", msg.text);
        //    });

        //    slashCommandHandler.ShouldHaveReceived(m => Equivalent(msg, m), 1.Seconds());
        //}

        //private static bool Equivalent(SlashCommand c1, SlashCommand c2)
        //{
        //    return c1.team_id == c2.team_id &&
        //           c1.channel_id == c2.channel_id &&
        //           c1.channel_name == c2.channel_name &&
        //           c1.user_id == c2.user_id &&
        //           c1.user_name == c2.user_name &&
        //           c1.command == c2.command &&
        //           c1.text == c2.text;
        //}
    }
}
