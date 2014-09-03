namespace SlackCommander.Web.Commands
{
    public class MessageToSlack : ICommand
    {
        public string username { get; set; }
        public string icon_url { get; set; }
        public string icon_emoji { get; set; }
        public string channel { get; set; }
        public string text { get; set; }
        public bool unfurl_links { get; set; }
        public Attachment[] attachments { get; set; }

        public MessageToSlack()
        {
            attachments = new Attachment[0];
        }

        public class Attachment
        {
            public string fallback { get; set; }
            public string pretext { get; set; }
            public string text { get; set; }
            public string color { get; set; }
            public string[] markdwn_in { get; set; }
            public Field[] fields { get; set; }

            public Attachment()
            {
                markdwn_in = new string[0];
                fields = new Field[0];
            }

            public class Field
            {
                public string title { get; set; }
                public string value { get; set; }
                public bool @short { get; set; }
            }
        }
    }
}