using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class SagaAzureStorageLocker : ISagaLocker
    {

        const string CONTAINER_NAME = "afblocks";
        TimeSpan LOCK_DURATION = new TimeSpan(0, 0, 15);
        CloudStorageAccount storageAccount;

        public SagaAzureStorageLocker()
        {
            storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
        }

        
        public async Task CreateLocksContainer()
        {            

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

        }

        public async Task<string> CreateLock(string sagaId)
        {
            var sagaIdToGuid = StringToGuid(sagaId);          

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container called 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME);

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(sagaId);

            blob.UploadText(sagaId);

            var leaseId = string.Empty;

            try
            {
                leaseId = await blob.AcquireLeaseAsync(LOCK_DURATION);
            }
            catch (StorageException)
            {
                Random rnd = new Random();
                await Task.Delay(rnd.Next(0, 1000));

                throw;
            }          

            return leaseId;
        }



        public async Task ReleaseLock(string sagaId, string leaseId)
        {                                 

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container. 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME);

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(sagaId);

            AccessCondition acc = new AccessCondition();
            acc.LeaseId = leaseId;

            try
            {
                await blob.ReleaseLeaseAsync(acc);
            }
            catch (StorageException)
            {
                Random rnd = new Random();
                await Task.Delay(rnd.Next(0, 1000));

                throw;
            }

            
        }

        public async Task DeleteLock(string sagaId, string leaseId)
        {           

            // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container. 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME);

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(sagaId);

            AccessCondition acc = new AccessCondition();
            acc.LeaseId = leaseId;
            
            try
            {
                await blob.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, acc, null, null);
            }
            catch (StorageException)
            {
                Random rnd = new Random();
                await Task.Delay(rnd.Next(0, 1000));

                throw;
            }
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