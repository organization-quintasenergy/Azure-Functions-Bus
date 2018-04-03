using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class SagaAzureStoragePersistence : ISagaStoragePersistence
    {
        private const string TABLE_NAME = "sagapersistence";

        ISagaLocker sagaLock;
        bool lockSagas;

        public SagaAzureStoragePersistence(ISagaLocker sagaLock, bool lockSagas)
        {
            this.sagaLock = sagaLock;
            this.lockSagas = lockSagas;
        }

        public async Task CreateSagaPersistenceTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync();
        }

        public async Task Insert(SagaData entity)
        {
            var sagaID = entity.PartitionKey + entity.RowKey;
            var lockID = string.Empty;

            if (this.lockSagas)
            {              
                lockID = await sagaLock.CreateLock(sagaID);               
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            entity.CreationTimeStamp = DateTime.UtcNow;

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                        
            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity as ITableEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(insertOperation);

            if (this.lockSagas)
            {               
                await sagaLock.ReleaseLock(sagaID, lockID);
            }
        }

        public async Task Update(SagaData entity)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Create the TableOperation object that inserts the customer entity.
            TableOperation replaceOperation = TableOperation.Replace(entity as ITableEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(replaceOperation);

            var sagaID = entity.PartitionKey + entity.RowKey;

            if(this.lockSagas && !entity.IsDeleted)
                await sagaLock.ReleaseLock(sagaID, entity.LockID);
        }

        public async Task<T> GetSagaData<T>(string partitionKey, string rowKey) where T :SagaData
        {
            var sagaID = partitionKey + rowKey;
            var lockID = string.Empty;

            if (this.lockSagas)
            {
                lockID = await sagaLock.CreateLock(sagaID);
            }


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);            
            
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey,rowKey);

            // Execute the operation.
            var execution = await table.ExecuteAsync(retrieveOperation);

            var result = execution.Result as T;

            if (result != null && this.lockSagas)
                result.LockID = lockID;

            if (result == null && this.lockSagas)
            {
                await sagaLock.ReleaseLock(sagaID, lockID);
            }

            return result;
        }

        public async Task Delete(SagaData entity)
        {           
            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Create the TableOperation object that inserts the customer entity.
            TableOperation replaceOperation = TableOperation.Delete(entity);

            // Execute the insert operation.
            await table.ExecuteAsync(replaceOperation);

            /*entity.IsDeleted = true;
            entity.FinishingTimeStamp = DateTime.UtcNow;*/

            var sagaID = entity.PartitionKey + entity.RowKey;

            if (this.lockSagas)
            {
                await sagaLock.DeleteLock(sagaID, entity.LockID);
            }
        }
    }
}
