using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Sagas
{
    public interface HandlerWithCorrelation<SagaData,MessageType>
    {
        Task InvokeAsync(IBus bus, MessageType input, ITraceWriter Log);

        SagaData CorrelationExpression(MessageType m);
    }
}
