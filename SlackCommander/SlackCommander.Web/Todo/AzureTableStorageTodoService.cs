using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using Magnum.Extensions;
using MassTransit.Exceptions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using NLog;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace SlackCommander.Web.Todo
{
    public class AzureTableStorageTodoService : ITodoService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IAppSettings _appSettings;

        private static class Tables
        {
            public const string TodoLists = "todoLists";
        }

        public AzureTableStorageTodoService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public IEnumerable<TodoItem> GetItems(string userId, string listId)
        {
            try
            {
                var table = GetTable(Tables.TodoLists);
                return table.GetRecords<TodoItemRecord>(listId)
                    .Select(record => record.ToTodoItem())
                    .OrderBy(i => i.CreatedAt);
            }
            catch (Exception ex)
            {
                Log.Error("Get todo list failed.", ex);
                throw;
            }
        }

        public void AddItem(string userId, string listId, string text)
        {
            try
            {
                var table = GetTable(Tables.TodoLists);
                var record = new TodoItemRecord
                {
                    ListId = listId,
                    Text = text,
                    Done = false,
                    CreatedAt = DateTime.UtcNow
                };
                var itemId = 1;
                while (true)
                {
                    try
                    {
                        record.Id = itemId++.ToString();
                        table.InsertRecord(record);
                        break;
                    }
                    catch (StorageException ex)
                    {
                        // Swallow the exception and retry if the insert failed due to a conflict.
                        // Conflicts are expected since ItemId always starts at 1.
                        if (ex.RequestInformation != null &&
                            ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                        {
                            continue;
                        }
                        throw; // Exception was not a conflict - just throw and let the insert fail.
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Add todo item failed.", ex);
                throw;
            }
        }

        public void TickItem(string userId, string listId, string itemId, bool force = false)
        {
            try
            {
                var table = GetTable(Tables.TodoLists);
                var record = table.GetRecord<TodoItemRecord>(listId, itemId);
                if (record == null || record.Done)
                {
                    return;
                }
                if (record.ClaimedBy.Missing() ||
                    record.ClaimedBy == userId ||
                    force)
                {
                    record.Done = true;
                    record.ClaimedBy = null;
                    table.ReplaceRecord(record);
                    return;
                }
                throw new TodoItemClaimedBySomeoneElseException(record.ClaimedBy);
            }
            catch (TodoItemClaimedBySomeoneElseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Mark todo item as done failed.", ex);
                throw;
            }
        }

        public void UntickItem(string userId, string listId, string itemId)
        {
            try
            {
                var table = GetTable(Tables.TodoLists);
                var record = table.GetRecord<TodoItemRecord>(listId, itemId);
                if (record == null || !record.Done)
                {
                    return;
                }
                record.Done = false;
                table.ReplaceRecord(record);
            }
            catch (Exception ex)
            {
                Log.Error("Mark todo item as not done failed.", ex);
                throw;
            }
        }

        public void RemoveItem(string userId, string listId, string itemId, bool force = false)
        {
            try
            {
                var table = GetTable(Tables.TodoLists);
                var record = table.GetRecord<TodoItemRecord>(listId, itemId);
                if (record == null)
                {
                    return;
                }
                if (record.ClaimedBy.Missing() ||
                    record.ClaimedBy == userId ||
                    force)
                {
                    table.DeleteRecord(record);
                    return;
                }
                throw new TodoItemClaimedBySomeoneElseException(record.ClaimedBy);
            }
            catch (TodoItemClaimedBySomeoneElseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Remove todo item failed.", ex);
                throw;
            }
        }

        public void ClearItems(string userId, string listId, bool includeUnticked = false, bool force = false)
        {
            try
            {
                var table = GetTable(Tables.TodoLists);
                var records = table.GetRecords<TodoItemRecord>(listId).Where(x => x.Done || includeUnticked).ToArray();
                if (!force && records.Any(r => !r.ClaimedBy.Missing() && r.ClaimedBy != userId))
                {
                    throw new TodoItemClaimedBySomeoneElseException(null);
                }
                foreach (var record in records)
                {
                    table.DeleteRecord(record);
                }
            }
            catch (TodoItemClaimedBySomeoneElseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Clear todo items failed.", ex);
                throw;
            }
        }

        public void ClaimItem(string userId, string listId, string itemId, bool force = false)
        {
            try
            {
                var listsTable = GetTable(Tables.TodoLists);
                var todoItemRecord = listsTable.GetRecord<TodoItemRecord>(listId, itemId);
                if (todoItemRecord == null ||
                    todoItemRecord.ClaimedBy == userId)
                {
                    return;
                }
                if (todoItemRecord.ClaimedBy.Missing() ||
                    force)
                {
                    todoItemRecord.ClaimedBy = userId;
                    listsTable.ReplaceRecord(todoItemRecord);
                    return;
                }
                throw new TodoItemClaimedBySomeoneElseException(todoItemRecord.ClaimedBy);
            }
            catch (TodoItemClaimedBySomeoneElseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Claim todo item failed.", ex);
                throw;
            }
        }

        public void FreeItem(string userId, string listId, string itemId, bool force = false)
        {
            try
            {
                var listsTable = GetTable(Tables.TodoLists);
                var todoItemRecord = listsTable.GetRecord<TodoItemRecord>(listId, itemId);
                if (todoItemRecord == null ||
                    todoItemRecord.ClaimedBy.Missing())
                {
                    return;
                }
                if (todoItemRecord.ClaimedBy == userId ||
                    force)
                {
                    todoItemRecord.ClaimedBy = null;
                    listsTable.ReplaceRecord(todoItemRecord);
                    return;
                }
                throw new TodoItemClaimedBySomeoneElseException(todoItemRecord.ClaimedBy);
            }
            catch (TodoItemClaimedBySomeoneElseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Free todo item failed.", ex);
                throw;
            }
        }

        private CloudTable GetTable(string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(_appSettings.Get("todo:azureStorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
            return table;
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
            public string ClaimedBy { get; set; }
            public DateTime CreatedAt { get; set; }

            public TodoItemRecord()
            {
            }

            public TodoItemRecord(TodoItem item)
            {
                ListId = item.ListId;
                Id = item.Id;
                Text = item.Text;
                Done = item.Done;
                ClaimedBy = item.ClaimedBy;
                CreatedAt = item.CreatedAt;
            }

            public TodoItem ToTodoItem()
            {
                return new TodoItem
                {
                    ListId = ListId,
                    Id = Id,
                    Text = Text,
                    Done = Done,
                    ClaimedBy = ClaimedBy,
                    CreatedAt = CreatedAt
                };
            }
        }
    }

    public static class CloudTableExtensions
    {
        public static TRecord GetRecord<TRecord>(this CloudTable table, string partitionKey, string rowKey)
            where TRecord : TableEntity
        {
            var retrieveOp = TableOperation.Retrieve<TRecord>(partitionKey, rowKey);
            return table.Execute(retrieveOp).Result as TRecord;
        }

        public static IEnumerable<TRecord> GetRecords<TRecord>(this CloudTable table, string partitionKey)
            where TRecord : TableEntity, new()
        {
            var query = new TableQuery<TRecord>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return table.ExecuteQuery(query);
        }

        public static void DeleteRecord<TRecord>(this CloudTable table, TRecord record)
            where TRecord : TableEntity
        {
            if (record != null)
            {
                var deleteOp = TableOperation.Delete(record);
                table.Execute(deleteOp);
            }
        }

        public static void DeleteRecord<TRecord>(this CloudTable table, string partitionKey, string rowKey)
            where TRecord : TableEntity
        {
            var record = table.GetRecord<TRecord>(partitionKey, rowKey);
            if (record != null)
            {
                var deleteOp = TableOperation.Delete(record);
                table.Execute(deleteOp);
            }
        }

        public static void InsertRecord<TRecord>(this CloudTable table, TRecord record) where TRecord : TableEntity
        {
            var insertOp = TableOperation.Insert(record);
            table.Execute(insertOp);
        }

        public static void ReplaceRecord<TRecord>(this CloudTable table, TRecord record) where TRecord : TableEntity
        {
            var replaceOp = TableOperation.Replace(record);
            table.Execute(replaceOp); // TODO Handle failure/retry
        }
    }
}