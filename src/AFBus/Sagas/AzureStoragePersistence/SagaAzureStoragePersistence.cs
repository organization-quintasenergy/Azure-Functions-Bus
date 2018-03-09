using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    class SagaAzureStoragePersistence : ISagaStoragePersistence
    {
        private const string TABLE_NAME = "sagapersistence";
            
        public async Task CreateSagaPersistenceTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync();
        }

        public async Task Insert(SagaData entity)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            entity.CreationTimeStamp = DateTime.UtcNow;

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                        
            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity as ITableEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(insertOperation);
        }

        public async Task Update(SagaData entity)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Create the TableOperation object that inserts the customer entity.
            TableOperation replaceOperation = TableOperation.Replace(entity as ITableEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(replaceOperation);
        }

        public async Task<T> GetSagaData<T>(string partitionKey, string rowKey) where T :SagaData
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);
            
            
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey,rowKey);

            // Execute the operation.
            var execution = await table.ExecuteAsync(retrieveOperation);

            return execution.Result as T;
        }

        public Task Delete(SagaData entity)
        {
            entity.IsDeleted = true;
            entity.FinishingTimeStamp = DateTime.UtcNow;

            return Task.CompletedTask;
            /*CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Create the TableOperation object that inserts the customer entity.
            TableOperation replaceOperation = TableOperation.Delete(entity);

            // Execute the insert operation.
            await table.ExecuteAsync(replaceOperation);*/
        }
    }
}
