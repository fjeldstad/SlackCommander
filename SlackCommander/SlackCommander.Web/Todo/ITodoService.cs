using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackCommander.Web.Todo
{
    public interface ITodoService
    {
        IEnumerable<TodoItem> GetItems(string userId, string listId);
        void AddItem(string userId, string listId, string text);
        void TickItem(string userId, string listId, string itemId, bool force = false);
        void UntickItem(string userId, string listId, string itemId);
        void RemoveItem(string userId, string listId, string itemId, bool force = false);
        void ClearItems(string userId, string listId, bool includeUnticked = false, bool force = false);
        void ClaimItem(string userId, string listId, string itemId, bool force = false);
        void FreeItem(string userId, string listId, string itemId, bool force = false);
    }
}
