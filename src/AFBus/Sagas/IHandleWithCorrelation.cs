using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandleWithCorrelation<MessageType>
    {
        Task HandleAsync(IBus bus, MessageType message, ITraceWriter Log);

        Task<SagaData> LookForInstance(ISagaStoragePersistence sagaPersistence, MessageType message);
    }
}
