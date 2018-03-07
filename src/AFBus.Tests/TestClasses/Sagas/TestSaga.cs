using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class TestSaga : Saga<TestSagaData>, IHandleStartingSaga<SagaStartingMessage>, IHandleWithCorrelation<SagaData,SagaIntermediateMessage>
    {
        public Task HandleAsync(IBus bus, SagaStartingMessage input, ITraceWriter Log)
        {
            //throw new NotImplementedException();

            this.Data.PartitionKey = "TestSaga";
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;

            return Task.CompletedTask;
        }


        async Task<SagaData> IHandleWithCorrelation<SagaData, SagaIntermediateMessage>.LookForInstance(ISagaStoragePersistence sagaPersistence, SagaIntermediateMessage message)
        {
            var sagaData =  await sagaPersistence.GetSagaData<TestSagaData>("TestSaga", message.Id.ToString());

            return sagaData;
        }

        Task IHandleWithCorrelation<SagaData, SagaIntermediateMessage>.HandleAsync(IBus bus, SagaIntermediateMessage input, ITraceWriter Log)
        {
            this.Data.Counter++;
            return Task.CompletedTask;
        }
    }
}
