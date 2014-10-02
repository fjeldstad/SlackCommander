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
            int idMaxLength = items.Max(i => i.Id.Length);
            var text = new StringBuilder();
            foreach (var item in items)
            {
                text.AppendLine(string.Format(
                "`{0}` {1} {2} {3} {4}",
                item.Id.PadLeft(idMaxLength),
                item.Done ? ":white_check_mark:" : ":white_square:",
                item.Text,
                item.ClaimedBy.Missing() ? string.Empty : ":lock:",
                item.ClaimedBy.Missing() ? string.Empty : string.Format("<@{0}>", item.ClaimedBy)).Trim());
            }
            return text.ToString().Trim();
        }
    }
}