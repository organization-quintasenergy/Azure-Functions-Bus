using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    /// <summary>
    /// Interface with the functions for managing the saga persistence.
    /// </summary>
    public interface ISagaStoragePersistence
    {
        Task CreateSagaPersistenceTableAsync();

        Task<T> GetSagaDataAsync<T>(string partitionKey, string rowKey) where T : SagaData;

        Task<List<T>> FindSagaDataAsync<T>(TableQuery<T> tableQuery) where T : SagaData, ITableEntity, new();

        Task InsertAsync(SagaData entity);

        Task UpdateAsync(SagaData entity);

        Task DeleteAsync(SagaData entity);

        /// <summary>
        /// Empty the related blob container.
        /// </summary>
        /// <param name="entity">SagaData</param>
        /// <returns></returns>
        Task DeleteBlobAsync(SagaData entity);

        /// <summary>
        /// Storing into the blob a big Saga Property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<T> StoreDataInBlob<T>(T property, SagaData entity);


        /// <summary>
        /// Loading the stored blob data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bigPropertyWrapperSerialized"></param>
        /// <returns></returns>
        Task<T> LoadDataFromBlob<T>(string bigPropertyWrapperSerialized);
    }
}
