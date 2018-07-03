using AFBus;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class ErrorTestSaga : Saga<ErrorTestSagaData>, IHandleCommandStartingSaga<ErrorSagaStartingMessage>, IHandleCommandWithCorrelation<ErrorSagaIntermediateMessage>, IHandleCommandWithCorrelation<ErrorSagaTerminatingMessage>
    {
        public static string PARTITION_KEY = "ErrorTestSaga";


        public Task HandleCommandAsync(IBus bus, ErrorSagaStartingMessage input, TraceWriter Log)
        {

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;
         

            return Task.CompletedTask;
        }

        public Task HandleCommandAsync(IBus bus, ErrorSagaIntermediateMessage input, TraceWriter Log)
        {
            throw new Exception();

           
        }

        public async Task HandleCommandAsync(IBus bus, ErrorSagaTerminatingMessage message, TraceWriter Log)
        {
            await this.DeleteSaga();
        }

        public async Task<SagaData> LookForInstanceAsync(ErrorSagaIntermediateMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaDataAsync<ErrorTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }

        public async Task<SagaData> LookForInstanceAsync(ErrorSagaTerminatingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaDataAsync<ErrorTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    
    }
}
