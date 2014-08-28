using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Refit;

namespace SlackCommander.Web.CommandHandlers
{
    public class Whois : ICommandHandler
    {
        private readonly string _fullContactApiBaseUrl;
        private readonly string _fullContactApiKey;
        private readonly string _fullContactWebhookUrl;
        private readonly IPendingCommands _pendingCommands;

        public Whois(IAppSettings appSettings, IPendingCommands pendingCommands)
        {
            _fullContactApiBaseUrl = appSettings.Get("fullContact:apiBaseUrl");
            _fullContactApiKey = appSettings.Get("fullContact:apiKey");
            _fullContactWebhookUrl = appSettings.Get("fullContact:webhookUrl");
            _pendingCommands = pendingCommands;
        }

        public async Task<dynamic> Handle(Command command)
        {
            if (!command.text.IsValidEmail() &&
                !command.text.CouldBeTwitterHandle())
            {
                return "Sorry, I'm only able to work with either e-mail addresses or Twitter handles.";
            }

            var commandId = Guid.NewGuid().ToString();
            var fullContactApi = RestService.For<IFullContactApi>(_fullContactApiBaseUrl);
            try
            {
                if (command.text.IsValidEmail())
                {
                    await fullContactApi.LookupByEmail(
                        command.text, 
                        _fullContactWebhookUrl, 
                        commandId, 
                        _fullContactApiKey);
                }
                else
                {
                    await fullContactApi.LookupByTwitterHandle(
                        command.text.TrimStart('@'), 
                        _fullContactWebhookUrl,
                        commandId, 
                        _fullContactApiKey);
                }
                _pendingCommands.Add(commandId, command);
                return string.Format("Looking up *{0}*, give me a few moments...", command.text);
            }
            catch
            {
                return string.Format("There was a problem with the lookup. I'm sorry.");
            }
        }
    }
}