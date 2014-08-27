using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlackCommander.Web
{
    public interface IAppSettings
    {
        string Get(string key);
    }
}
