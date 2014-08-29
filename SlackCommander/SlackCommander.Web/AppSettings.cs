using System.Configuration;

namespace SlackCommander.Web
{
    public class AppSettings : IAppSettings
    {
        public string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}