using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class EventTestSaga : Saga<EventTestSagaData>, IHandleCommandStartingSaga<EventSagaStartingMessage>,  IHandleEventWithCorrelation<EventSagaIntermediateMessage>, IHandleCommandWithCorrelation<EventSagaTerminatingMessage>
    {
        private const string PARTITION_KEY = "EventTestSaga";

        
        public Task HandleCommandAsync(IBus bus, EventSagaStartingMessage input, ILogger Log)
        {           

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;
          

            return Task.CompletedTask;
        }

      
        public Task HandleEventAsync(IBus bus, EventSagaIntermediateMessage message, ILogger log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(IBus bus, EventSagaTerminatingMessage message, ILogger Log)
        {
            await this.DeleteSagaAsync();
        }
             


        public async Task<SagaData> LookForInstanceAsync(EventSagaTerminatingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaDataAsync<EventTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }

        public async Task<List<SagaData>> LookForInstanceAsync(EventSagaIntermediateMessage message)
        {
            var tableQuery = new TableQuery<EventTestSagaData>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PARTITION_KEY));



            var sagasData = await SagaPersistence.FindSagaDataAsync<EventTestSagaData>(tableQuery);

            return sagasData.Select(sd => sd as SagaData).ToList();
        }

     
    }
}
