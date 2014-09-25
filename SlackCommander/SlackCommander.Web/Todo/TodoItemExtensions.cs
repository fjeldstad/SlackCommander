using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SlackCommander.Web.Todo
{
    public static class TodoItemExtensions
    {
        public static string ToSlackString(this IEnumerable<TodoItem> todoItems)
        {
            var items = todoItems.ToArray();
            if (!items.Any())
            {
                return "You are all done!";
            }
            var text = new StringBuilder();
            foreach (var item in items)
            {
                text.AppendLine(item.ToSlackString());
            }
            return text.ToString().Trim();
        }

        public static string ToSlackString(this TodoItem todoItem)
        {
            return string.Format(
                "`{0}` {1} {2}",
                todoItem.Id,
                todoItem.Done ? ":white_check_mark:" : ":white_square:",
                todoItem.Text);
        }
    }
}