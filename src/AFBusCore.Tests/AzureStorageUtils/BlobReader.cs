using AFBus;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests
{
    internal class BlobReader
    {
        static string CONTAINER_NAME = "bigmessages";

        public static async Task<IEnumerable<IListBlobItem>> ListFilesAsync()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            var blobList = await cloudBlobContainer.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.None, int.MaxValue, null, null, null);

            return blobList.Results;


        }

        public static async Task DeleteFilesAsync()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            await cloudBlobContainer.DeleteAsync();            


        }
    }
}
