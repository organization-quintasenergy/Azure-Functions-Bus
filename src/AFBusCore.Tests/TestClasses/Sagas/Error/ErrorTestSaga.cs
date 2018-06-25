using AFBus;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class ErrorTestSaga : Saga<ErrorTestSagaData>, IHandleStartingSaga<ErrorSagaStartingMessage>, IHandleWithCorrelation<ErrorSagaIntermediateMessage>, IHandleWithCorrelation<ErrorSagaTerminatingMessage>
    {
        public static string PARTITION_KEY = "ErrorTestSaga";


        public Task HandleAsync(IBus bus, ErrorSagaStartingMessage input, TraceWriter Log)
        {

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;
         

            return Task.CompletedTask;
        }

        public Task HandleAsync(IBus bus, ErrorSagaIntermediateMessage input, TraceWriter Log)
        {
            throw new Exception();

            return Task.CompletedTask;
        }

        public async Task HandleAsync(IBus bus, ErrorSagaTerminatingMessage message, TraceWriter Log)
        {
            await this.DeleteSaga();
        }

        public async Task<SagaData> LookForInstance(ErrorSagaIntermediateMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaData<ErrorTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }

        public async Task<SagaData> LookForInstance(ErrorSagaTerminatingMessage message)
        {
            var sagaData = await SagaPersistence.GetSagaData<ErrorTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    
    }
}
