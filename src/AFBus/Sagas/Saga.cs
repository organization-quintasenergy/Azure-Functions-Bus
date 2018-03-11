using Microsoft.WindowsAzure.Storage.Table;
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
    public abstract class Saga<T> where T: SagaData,new()
    {
        protected ISagaStoragePersistence sagaPersistence;


        public Saga(ISagaStoragePersistence sagaPersistence = null)
        {
            Data = new T();

            this.sagaPersistence = sagaPersistence ?? new SagaAzureStoragePersistence(new SagaAzureStorageLocker());
        }

        public T Data { get; set; }

        /// <summary>
        /// Deletes the saga in the persistence
        /// </summary>       
        public async Task DeleteSaga()
        {
            await sagaPersistence.Delete(Data);

        }
    }

    
}
