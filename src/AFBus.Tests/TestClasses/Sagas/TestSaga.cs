using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public class TestSaga : Saga<SagaData>, IHandleStartingSaga<SagaStartingMessage>
    {
        public Task HandleAsync(IBus bus, SagaStartingMessage input, ITraceWriter Log)
        {
            throw new NotImplementedException();
        }
    }
}
