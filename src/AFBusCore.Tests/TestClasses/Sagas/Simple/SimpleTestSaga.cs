using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class SimpleTestSaga : Saga<SimpleTestSagaData>, IHandleCommandStartingSaga<SimpleSagaStartingMessage>,  IHandleCommandWithCorrelation<SimpleSagaIntermediateMessage>, IHandleCommandWithCorrelation<SimpleSagaTerminatingMessage>
    {
        private const string PARTITION_KEY = "SimpleTestSaga";

        
        public Task HandleCommandAsync(IBus bus, SimpleSagaStartingMessage input, ILogger Log)
        {           

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }

        public Task HandleCommandAsync(IBus bus, SimpleSagaIntermediateMessage input, ILogger Log)
        {
            this.Data.Counter++;

            InvocationCounter.Instance.AddOne();
            
            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(IBus bus, SimpleSagaTerminatingMessage message, ILogger Log)
        {
            await this.DeleteSagaAsync();
        }

        public async Task<SagaData> LookForInstanceAsync(SimpleSagaIntermediateMessage message)
        {
            var sagaData =  await SagaPersistence.GetSagaDataAsync<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }

        public async Task<SagaData> LookForInstanceAsync(SimpleSagaTerminatingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaDataAsync<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
}
