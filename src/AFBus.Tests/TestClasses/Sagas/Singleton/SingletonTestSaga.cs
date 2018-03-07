using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class SingletonTestSaga : Saga<SingletonTestSagaData>, IHandleStartingSaga<SingletonSagaStartingMessage>, IHandleWithCorrelation<SingletonSagaStartingMessage>
    {
        private const string PARTITION_KEY = "SingletonTestSaga";

        Task IHandleStartingSaga<SingletonSagaStartingMessage>.HandleAsync(IBus bus, SingletonSagaStartingMessage message, ITraceWriter Log)
        {
            Data.PartitionKey = PARTITION_KEY;
            Data.RowKey = message.Id.ToString();

            Data.Counter = 1;

            return Task.CompletedTask;
        }

        Task IHandleWithCorrelation<SingletonSagaStartingMessage>.HandleAsync(IBus bus, SingletonSagaStartingMessage message, ITraceWriter Log)
        {

            return Task.CompletedTask;
        }

        async Task<SagaData> IHandleWithCorrelation<SingletonSagaStartingMessage>.LookForInstance(ISagaStoragePersistence sagaPersistence, SingletonSagaStartingMessage message)
        {
            var sagaData = await sagaPersistence.GetSagaData<SingletonTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
}
