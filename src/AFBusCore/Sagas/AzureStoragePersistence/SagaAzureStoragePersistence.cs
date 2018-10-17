using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
        private const string CONTAINER_NAME = "bigpropertiesstorage";

        ISagaLocker sagaLock;
        private bool lockSagas;
        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
        static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        static CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

        public bool LockSagas { get => lockSagas; set => lockSagas = value; }

        public SagaAzureStoragePersistence(ISagaLocker sagaLock, bool lockSagas)
        {
            this.sagaLock = sagaLock;
            this.LockSagas = lockSagas;
            
        }

        public async Task CreateSagaPersistenceTableAsync()
        {
            
            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            // Create the table if it doesn't exist.
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        public async Task InsertAsync(SagaData entity)
        {
            var sagaID = entity.Prefix;
            var lockID = string.Empty;

            if (this.LockSagas)
            {
                lockID = await sagaLock.CreateLock(sagaID).ConfigureAwait(false);
            }

            entity.CreationTimeStamp = DateTime.UtcNow;
          

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity as ITableEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(insertOperation).ConfigureAwait(false);

            if (this.LockSagas)
            {
                await sagaLock.ReleaseLock(sagaID, lockID).ConfigureAwait(false);
            }
        }

        public async Task UpdateAsync(SagaData entity)
        {
            if (entity.IsDeleted)
                return;


            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Create the TableOperation object that inserts the customer entity.
            TableOperation replaceOperation = TableOperation.Replace(entity as ITableEntity);

            // Execute the insert operation.
            await table.ExecuteAsync(replaceOperation).ConfigureAwait(false);

            var sagaID = entity.Prefix;

            if (this.LockSagas && !entity.IsDeleted)
                await sagaLock.ReleaseLock(sagaID, entity.LockID).ConfigureAwait(false);
        }

        public async Task<T> GetSagaDataAsync<T>(string partitionKey, string rowKey) where T : SagaData
        {
            var sagaID = partitionKey + rowKey;
            var lockID = string.Empty;

            if (this.LockSagas)
            {
                lockID = await sagaLock.CreateLock(sagaID).ConfigureAwait(false);
            }


            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            // Execute the operation.
            var execution = await table.ExecuteAsync(retrieveOperation).ConfigureAwait(false);

            var result = execution.Result as T;

            if (result != null && this.LockSagas)
                result.LockID = lockID;

            if (result == null && this.LockSagas)
            {
                await sagaLock.ReleaseLock(sagaID, lockID).ConfigureAwait(false);
            }

            return result;
        }

        public async Task DeleteBlobAsync(SagaData entity)
        {
            var jsonSerializer = new JSONSerializer();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            //Get all the items into the container that contains the name (PartitionKey + RowKey) 
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await cloudBlobContainer.ListBlobsSegmentedAsync(entity.Prefix, blobContinuationToken);
                
                //Delete all of them
                foreach (IListBlobItem item in results.Results)
                {
                    System.Diagnostics.Debug.WriteLine(item.Uri);
                    CloudBlockBlob block = new CloudBlockBlob(item.Uri);
                    CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(block.Name);
                    //NO ENTIENDO LA DIFERENCIA ENTRE LAS DOS LINEAS DE ARRIBA PERO NO FUNCIONA EL DELETE DEL BLOCK DE LA 152.

                    await blockBlob.DeleteIfExistsAsync();
                }
            } while (blobContinuationToken != null);
        }

        public async Task DeleteAsync(SagaData entity)
        {
            entity.IsDeleted = true;
            entity.FinishingTimeStamp = DateTime.UtcNow;

            var sagaID = entity.Prefix;

            if (this.LockSagas)
            {
                await sagaLock.DeleteLock(sagaID, entity.LockID).ConfigureAwait(false);
            }


            CloudTable table = tableClient.GetTableReference(TABLE_NAME);

            // Create the TableOperation object that inserts the customer entity.
            TableOperation replaceOperation = TableOperation.Delete(entity);

            // Execute the insert operation.
            await table.ExecuteAsync(replaceOperation);
            /*
            entity.IsDeleted = true;
            entity.FinishingTimeStamp = DateTime.UtcNow;

            var sagaID = entity.PartitionKey + entity.RowKey;

            if (this.lockSagas)
            {
                await sagaLock.DeleteLock(sagaID, entity.LockID);
            }*/


        }

        public async Task<List<T>> FindSagaDataAsync<T>(TableQuery<T> tableQuery) where T : SagaData, ITableEntity, new()
        {

            CloudTable table = tableClient.GetTableReference(TABLE_NAME);


            // Execute the operation.
            var result = await table.ExecuteQueryAsync(tableQuery).ConfigureAwait(false);


            return result;
        }

        public class BigPropertyWrapper
        {
            public string FileName { get; set; }

            public string PropertyType { get; set; }
        }

        public async Task<string> StoreDataInBlob<T>(T property, SagaData entity)
        {
            var jsonSerializer = new JSONSerializer();
            var wrapper = new BigPropertyWrapper();

            var fileName = entity.Prefix + "-" + Guid.NewGuid().ToString("N").ToLower() + ".afbus";

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            await blockBlob.UploadTextAsync(jsonSerializer.Serialize(property));

            wrapper.PropertyType = typeof(T).AssemblyQualifiedName;
            wrapper.FileName = blockBlob.Name;

            return jsonSerializer.Serialize(wrapper);
        }

        public async Task<T> LoadDataFromBlob<T>(string bigPropertyWrapperSerialized)
        {
            var jsonSerializer = new JSONSerializer();
            var wrapper = jsonSerializer.Deserialize(bigPropertyWrapperSerialized, typeof(BigPropertyWrapper)) as BigPropertyWrapper;

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(wrapper.FileName);

            var fileContent = await blockBlob.DownloadTextAsync();

            return (T)jsonSerializer.Deserialize(fileContent, Type.GetType(wrapper.PropertyType));
        }
    }
}
