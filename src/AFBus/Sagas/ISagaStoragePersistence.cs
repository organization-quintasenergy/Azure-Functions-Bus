using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface ISagaStoragePersistence
    {
        Task CreateSagaPersistenceTable();

        Task Insert(SagaData entity);

        Task Update(SagaData entity);

        Task<T> GetSagaData<T>(string partitionKey, string rowKey) where T : SagaData;
    }
}
