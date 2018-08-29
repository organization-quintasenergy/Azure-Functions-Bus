
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class StorePropertyInBlobUtil
    {
        private static readonly string CONTAINER_NAME = "bigpropertiesstorage";

        public static async Task<T> StoreDataInBlob<T>(T property)
        {
            var jsonSerializer = new JSONSerializer();
            var wrapper = new BigPropertyWrapper();                       

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            var fileName = Guid.NewGuid().ToString("N").ToLower() + ".afbus";

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            await blockBlob.UploadTextAsync(jsonSerializer.Serialize(property));

            wrapper.PropertyType = typeof(T).AssemblyQualifiedName;
            wrapper.FileName = blockBlob.Name;

            return (T)jsonSerializer.Deserialize(jsonSerializer.Serialize(property), Type.GetType(wrapper.PropertyType));
        }

        public static async Task<T> LoadDataFromBlob<T>(string bigPropertyWrapperSerialized)
        {
            var jsonSerializer = new JSONSerializer();
            var wrapper = jsonSerializer.Deserialize(bigPropertyWrapperSerialized,typeof(BigPropertyWrapper)) as BigPropertyWrapper;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                       

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(wrapper.FileName);

            var fileContent=await blockBlob.DownloadTextAsync();
                       
            return (T)jsonSerializer.Deserialize(fileContent, Type.GetType(wrapper.PropertyType));

        }

        public static async Task<bool> DeleteBlob(string bigPropertyWrapperSerialized)
        {
            var jsonSerializer = new JSONSerializer();
            var wrapper = jsonSerializer.Deserialize(bigPropertyWrapperSerialized, typeof(BigPropertyWrapper)) as BigPropertyWrapper;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();


            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(wrapper.FileName);

            return await blockBlob.DeleteIfExistsAsync();
           
        }

        public class BigPropertyWrapper
        {
            public string FileName { get; set; }

            public string PropertyType { get; set; }
        }
    }

    
}
