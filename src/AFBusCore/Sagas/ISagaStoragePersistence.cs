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
        Task CreateSagaPersistenceTable();

        Task<T> GetSagaData<T>(string partitionKey, string rowKey) where T : SagaData;

        Task Insert(SagaData entity);

        Task Update(SagaData entity);

        Task Delete(SagaData entity);
    }
}
