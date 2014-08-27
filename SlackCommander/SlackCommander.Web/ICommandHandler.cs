using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlackCommander.Web
{
    public interface ICommandHandler
    {
        dynamic Handle(Command command);
    }
}
