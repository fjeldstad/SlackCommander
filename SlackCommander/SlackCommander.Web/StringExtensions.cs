using System;
using System.Linq;
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

        public static string SubstringByWords(this string value, int startWordIndex, int? numberOfWords = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            var words = value.Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= startWordIndex)
            {
                return string.Empty;
            }
            var wordsToTake = Math.Min(words.Length - startWordIndex, numberOfWords ?? words.Length);
            return string.Join(" ", words.Skip(startWordIndex).Take(wordsToTake)).Trim();
        }
    }
}