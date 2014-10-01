using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Todo
{
    public class TodoItemClaimedBySomeoneElseException : Exception
    {
        public string UserId { get; set; }

        public TodoItemClaimedBySomeoneElseException(string userId)
        {
            UserId = userId;
        }
    }
}