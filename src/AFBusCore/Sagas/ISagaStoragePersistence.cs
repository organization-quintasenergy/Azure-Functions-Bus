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
    }
}
