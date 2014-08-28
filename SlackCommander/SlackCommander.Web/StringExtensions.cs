using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

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
    }
}