using Microsoft.Azure.WebJobs.Host;
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

        Task IHandleStartingSaga<SingletonSagaStartingMessage>.HandleAsync(IBus bus, SingletonSagaStartingMessage message, TraceWriter Log)
        {
            Data.PartitionKey = PARTITION_KEY;
            Data.RowKey = message.Id.ToString();

            Data.Counter = 1;

            return Task.CompletedTask;
        }

        Task IHandleWithCorrelation<SingletonSagaStartingMessage>.HandleAsync(IBus bus, SingletonSagaStartingMessage message, TraceWriter Log)
        {

            return Task.CompletedTask;
        }

        async Task<SagaData> IHandleWithCorrelation<SingletonSagaStartingMessage>.LookForInstance(SingletonSagaStartingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaData<SingletonTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
}
