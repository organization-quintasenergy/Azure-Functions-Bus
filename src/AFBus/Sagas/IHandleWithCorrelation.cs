using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandleWithCorrelation<SagaData,MessageType>
    {
        Task HandleAsync(IBus bus, MessageType input, ITraceWriter Log);

        Task<SagaData> LookForInstance(ISagaStoragePersistence sagaPersistence, MessageType message);
    }
}
