using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class SingletonTestSaga : Saga<SingletonTestSagaData>, IHandleCommandStartingSaga<SingletonSagaStartingMessage>, IHandleCommandWithCorrelation<SingletonSagaStartingMessage>
    {
        private const string PARTITION_KEY = "SingletonTestSaga";

        Task IHandleCommandStartingSaga<SingletonSagaStartingMessage>.HandleCommandAsync(IBus bus, SingletonSagaStartingMessage message, TraceWriter Log)
        {
            Data.PartitionKey = PARTITION_KEY;
            Data.RowKey = message.Id.ToString();

            Data.Counter = 1;

            return Task.CompletedTask;
        }

        Task IHandleCommandWithCorrelation<SingletonSagaStartingMessage>.HandleCommandAsync(IBus bus, SingletonSagaStartingMessage message, TraceWriter Log)
        {

            return Task.CompletedTask;
        }

        async Task<SagaData> IHandleCommandWithCorrelation<SingletonSagaStartingMessage>.LookForInstance(SingletonSagaStartingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaData<SingletonTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
}
