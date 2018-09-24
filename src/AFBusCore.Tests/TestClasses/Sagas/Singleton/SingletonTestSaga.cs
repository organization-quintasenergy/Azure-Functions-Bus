using Microsoft.Extensions.Logging;
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

        public Task HandleCommandAsync(IBus bus, SingletonSagaStartingMessage message, ILogger Log)
        {
            Data.PartitionKey = PARTITION_KEY;
            Data.RowKey = message.Id.ToString();

            Data.Counter = 1;

            return Task.CompletedTask;
        }

       

        public async Task<SagaData> LookForInstanceAsync(SingletonSagaStartingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaDataAsync<SingletonTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
}
