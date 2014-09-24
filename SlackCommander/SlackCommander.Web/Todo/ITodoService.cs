using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackCommander.Web.Todo
{
    public interface ITodoService
    {
        IEnumerable<TodoItem> GetItems(string listId);
        void AddItem(string listId, string text);
        void MarkItemAsDone(string listId, string itemId);
        void MarkItemAsNotDone(string listId, string itemId);
        void RemoveItem(string listId, string itemId);
        void ClearDoneItems(string listId);
    }
}
