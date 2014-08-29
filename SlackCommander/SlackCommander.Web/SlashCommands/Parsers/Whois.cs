using SlackCommander.Web.Commands;

namespace SlackCommander.Web.SlashCommands.Parsers
{
    public class Whois : SlashCommandParserBase
    {
        public const string Command = "/whois";

        public Whois() : base(Command) { }

        protected override ICommand ParseCore(SlashCommand slashCommand)
        {
            if (slashCommand.text.IsValidEmail())
            {
                return new WhoisEmail
                {
                    EmailAddress = slashCommand.text,
                    RequestedByUser = slashCommand.user_name,
                    RespondToChannel =
                        slashCommand.channel_name == "directmessage" ? 
                        "@" + slashCommand.user_name : 
                        "#" + slashCommand.channel_name
                };
            }

            if (slashCommand.text.CouldBeTwitterHandle())
            {
                return new WhoisTwitter
                {
                    TwitterHandle = slashCommand.text,
                    RequestedByUser = slashCommand.user_name,
                    RespondToChannel =
                        slashCommand.channel_name == "directmessage" ?
                        "@" + slashCommand.user_name :
                        "#" + slashCommand.channel_name
                };
            }

            throw new InvalidSlashCommandException("Sorry, I'm only able to work with e-mail addresses and Twitter handles.");
        }
    }
}