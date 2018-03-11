using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    class SagaAzureStorageLocker : ISagaLocker
    {

        const string CONTAINER_NAME = "afblocks";
        TimeSpan LOCK_DURATION = new TimeSpan(0, 0, 15);

        public async Task CreateLocksContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync();

        }

        public async Task<string> CreateLock(string sagaId)
        {
            var sagaIdToGuid = StringToGuid(sagaId);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container called 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME);

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(sagaId);
            blob.UploadText(sagaId);
            return await blob.AcquireLeaseAsync(LOCK_DURATION, sagaIdToGuid);
        }



        public async Task ReleaseLock(string sagaId, string leaseId)
        {           

            leaseId = StringToGuid(leaseId);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container called 'quickstartblobs'. 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME);

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(sagaId);

            AccessCondition acc = new AccessCondition();
            acc.LeaseId = leaseId;
            
            await blob.ReleaseLeaseAsync(acc);
        }

        private string StringToGuid(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                Guid result = new Guid(hash);

                return result.ToString();
            }
        }
    }
    

}