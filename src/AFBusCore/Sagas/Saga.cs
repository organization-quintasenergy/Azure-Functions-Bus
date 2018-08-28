using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    /// <summary>
    /// Base class for defining sagas
    /// </summary>
    /// <typeparam name="T">Type of the saga data stored in the persistence</typeparam>
    public abstract class Saga<T, T2> where T: SagaData,new()
    {
        

        public Saga()
        {
            Data = new T();
           
        }

        public T Data { get; set; }

        public ISagaStoragePersistence SagaPersistence { get; set; }

        /// <summary>
        /// Deletes the saga in the persistence
        /// </summary>       
        public async Task DeleteSagaAsync()
        {
            await SagaPersistence.DeleteAsync(Data);
            await SagaPersistence.DeleteBlobAsync(Data);
        }

        /// <summary>
        /// To write a property to the blob storage.
        /// </summary>
        /// <param name="property">Respective property</param>
        /// <returns></returns>
        public async Task<T2> WriteBlobProperty(List<T2> property)
        {
            return await SagaPersistence.StoreDataInBlob(property, Data);
        }
    }

    
}
