using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class SimpleTestSaga : Saga<SimpleTestSagaData>, IHandleStartingSaga<SimpleSagaStartingMessage>,  IHandleWithCorrelation<SimpleSagaIntermediateMessage>, IHandleWithCorrelation<SimpleSagaTerminatingMessage>
    {
        private const string PARTITION_KEY = "SimpleTestSaga";

        
        public Task HandleAsync(IBus bus, SimpleSagaStartingMessage input, TraceWriter Log)
        {           

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;

            return Task.CompletedTask;
        }

        public Task HandleAsync(IBus bus, SimpleSagaIntermediateMessage input, TraceWriter Log)
        {
            this.Data.Counter++;
            return Task.CompletedTask;
        }

        public async Task HandleAsync(IBus bus, SimpleSagaTerminatingMessage message, TraceWriter Log)
        {
            await this.DeleteSaga();
        }

        public async Task<SagaData> LookForInstance(SimpleSagaIntermediateMessage message)
        {
            var sagaData =  await sagaPersistence.GetSagaData<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }

        public async Task<SagaData> LookForInstance(SimpleSagaTerminatingMessage message)
        {
            var sagaData = await sagaPersistence.GetSagaData<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
}
