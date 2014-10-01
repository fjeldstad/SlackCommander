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
        void TickItem(string listId, string itemId);
        void UntickItem(string listId, string itemId);
        void RemoveItem(string listId, string itemId);
        void ClearItems(string listId, bool includeUnticked = false);
        void ClaimItem(string listId, string itemId, string userId);
        void FreeItem(string listId, string itemId, string userId);
    }
}
