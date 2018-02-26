using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFUtils.IoC;

namespace AFUtils.Tests.TestClasses
{
    public class TestMessageHandler : IFunction<TestMessage>
    {       

        public Task InvokeAsync(TestMessage input, ITraceWriter Log)
        {
            throw new NotImplementedException();
        }
    }
}
