using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class SimpleTestSaga : Saga<SimpleTestSagaData>, IHandleStartingSaga<SimpleSagaStartingMessage>,  IHandleWithCorrelation<SimpleSagaIntermediateMessage>
    {
        private const string PARTITION_KEY = "SimpleTestSaga";

        Task IHandleStartingSaga<SimpleSagaStartingMessage>.HandleAsync(IBus bus, SimpleSagaStartingMessage input, ITraceWriter Log)
        {           

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;

            return Task.CompletedTask;
        }

        Task IHandleWithCorrelation<SimpleSagaIntermediateMessage>.HandleAsync(IBus bus, SimpleSagaIntermediateMessage input, ITraceWriter Log)
        {
            this.Data.Counter++;
            return Task.CompletedTask;
        }

        async Task<SagaData> IHandleWithCorrelation<SimpleSagaIntermediateMessage>.LookForInstance(ISagaStoragePersistence sagaPersistence, SimpleSagaIntermediateMessage message)
        {
            var sagaData =  await sagaPersistence.GetSagaData<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
                
    }
}
