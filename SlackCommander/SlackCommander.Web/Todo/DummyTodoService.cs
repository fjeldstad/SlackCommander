using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using Magnum.Extensions;

namespace SlackCommander.Web.Todo
{
    public class DummyTodoService : ITodoService
    {
        private const string KeyPrefix = "DummyTodoService_";

        public IEnumerable<TodoItem> GetItems(string listId)
        {
            return GetList(listId);
        }

        public void AddItem(string listId, string text)
        {
            var list = GetList(listId).ToList();
            list.Add(new TodoItem
            {
                Text = text
            });
            SaveList(listId, list);
        }

        public void MarkItemAsDone(string listId, string itemId)
        {
            var list = GetList(listId).ToArray();
            var item = list.FirstOrDefault(i => i.Id.Equals(itemId, StringComparison.InvariantCultureIgnoreCase));
            if (item != null)
            {
                item.Done = true;
                SaveList(listId, list);
            }
        }

        public void RemoveItem(string listId, string itemId)
        {
            var list = GetList(listId).ToList();
            list.RemoveAll(item => item.Id.Equals(itemId, StringComparison.InvariantCultureIgnoreCase));
            SaveList(listId, list);
        }

        public void ClearItems(string listId)
        {
            SaveList(listId, new TodoItem[0]);
        }

        private static IEnumerable<TodoItem> GetList(string listId)
        {
            return MemoryCache.Default.Get(KeyPrefix + listId) as IEnumerable<TodoItem> ?? Enumerable.Empty<TodoItem>();
        }

        private static void SaveList(string listId, IEnumerable<TodoItem> items)
        {
            var list = items.ToList();
            var itemId = 1;
            foreach (var item in list)
            {
                item.Id = itemId++.ToString();
            }
            MemoryCache.Default.Set(KeyPrefix + listId, list, DateTimeOffset.MaxValue);
        }
    }
}