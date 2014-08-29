using System.Net.Mail;

namespace SlackCommander.Web
{
    public static class StringExtensions
    {
        public static bool IsValidEmail(this string value)
        {
            try
            {
                new MailAddress(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CouldBeTwitterHandle(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.StartsWith("@");
        }

        public static bool Missing(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}