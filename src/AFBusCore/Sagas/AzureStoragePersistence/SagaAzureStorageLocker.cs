﻿using Microsoft.WindowsAzure.Storage;
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
        TimeSpan LOCK_DURATION = new TimeSpan(0, 1, 0);
        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
        static CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
        static bool containerCreated = false;

        int MIN_WAITING_FOR_LOCK_RELEASING = 1000;
        int MAX_WAITING_FOR_LOCK_RELEASING = 2000;


        
        public async Task CreateLocksContainer()
        {
            // Create a container 
            if (!containerCreated)
            {
               
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
                await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);
                containerCreated = true;
            }

        }

        public async Task<string> CreateLock(string sagaId)
        {
            var sagaIdToGuid = StringToGuid(sagaId);          


            // Create a container called 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME);

            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(sagaId);

            await blob.UploadTextAsync(sagaId);

            var leaseId = string.Empty;

            try
            {
                leaseId = await blob.AcquireLeaseAsync(LOCK_DURATION);
            }
            catch (StorageException ex)
            {
                Random rnd = new Random();
                await Task.Delay(rnd.Next(MIN_WAITING_FOR_LOCK_RELEASING, MAX_WAITING_FOR_LOCK_RELEASING));

                throw;
            }          

            return leaseId;
        }



        public async Task ReleaseLock(string sagaId, string leaseId)
        {                                 


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
                await Task.Delay(rnd.Next(MIN_WAITING_FOR_LOCK_RELEASING, MAX_WAITING_FOR_LOCK_RELEASING));

                throw;
            }

            
        }

        public async Task DeleteLock(string sagaId, string leaseId)
        {           


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
                await Task.Delay(rnd.Next(MIN_WAITING_FOR_LOCK_RELEASING, MAX_WAITING_FOR_LOCK_RELEASING));

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