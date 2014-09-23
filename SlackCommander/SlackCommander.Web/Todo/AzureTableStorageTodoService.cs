using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Magnum.Extensions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using NLog;

namespace SlackCommander.Web.Todo
{
    public class AzureTableStorageTodoService : ITodoService
    {
        private const string TableName = "todoLists";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IAppSettings _appSettings;

        public AzureTableStorageTodoService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public IEnumerable<TodoItem> GetItems(string listId)
        {
            var table = GetTable();
            return GetRecords(table, listId).Select(record => record.ToTodoItem());
        }

        public void AddItem(string listId, string text)
        {
            var table = GetTable();
            var record = new TodoItemRecord
            {
                ListId = listId,
                Text = text,
                Done = false
            };
            var itemId = 1;
            while (true)
            {
                try
                {
                    record.Id = itemId++.ToString();
                    var insertOp = TableOperation.Insert(record);
                    table.Execute(insertOp);
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error("Insert todo item failed.", ex);
                }
            }
        }

        public void MarkItemAsDone(string listId, string itemId)
        {
            var table = GetTable();
            var record = GetRecord(table, listId, itemId);
            if (record == null || record.Done)
            {
                return;
            }
            record.Done = true;
            var replaceOp = TableOperation.Replace(record);
            table.Execute(replaceOp); // TODO Handle failure/retry
        }

        public void RemoveItem(string listId, string itemId)
        {
            var table = GetTable();
            var record = GetRecord(table, listId, itemId);
            if (record == null)
            {
                return;
            }
            var deleteOp = TableOperation.Delete(record);
            table.Execute(deleteOp); // TODO Handle failure/retry
        }

        public void ClearItems(string listId)
        {
            var table = GetTable();
            var records = GetRecords(table, listId);
            foreach (var record in records)
            {
                var deleteOp = TableOperation.Delete(record);
                table.Execute(deleteOp); // TODO Handle failure/retry
            }
        }

        private CloudTable GetTable()
        {
            var storageAccount = CloudStorageAccount.Parse(_appSettings.Get("azure:storageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(TableName);
            table.CreateIfNotExists();
            return table;
        }

        private IEnumerable<TodoItemRecord> GetRecords(CloudTable table, string listId)
        {
            var query = new TableQuery<TodoItemRecord>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, listId));
            return table.ExecuteQuery(query);
        } 

        private TodoItemRecord GetRecord(CloudTable table, string listId, string itemId)
        {
            var retrieveOp = TableOperation.Retrieve<TodoItemRecord>(listId, itemId);
            return table.Execute(retrieveOp).Result as TodoItemRecord;
        }

        public class TodoItemRecord : TableEntity
        {
            [IgnoreProperty]
            public string ListId
            {
                get { return PartitionKey; }
                set { PartitionKey = value; }
            }

            [IgnoreProperty]
            public string Id
            {
                get { return RowKey; }
                set { RowKey = value; }
            }

            public string Text { get; set; }
            public bool Done { get; set; }

            public TodoItemRecord()
            {
            }

            public TodoItemRecord(TodoItem item)
            {
                ListId = item.ListId;
                Id = item.Id;
                Text = item.Text;
                Done = item.Done;
            }

            public TodoItem ToTodoItem()
            {
                return new TodoItem
                {
                    ListId = ListId,
                    Id = Id,
                    Text = Text,
                    Done = Done
                };
            }
        }
    }
}