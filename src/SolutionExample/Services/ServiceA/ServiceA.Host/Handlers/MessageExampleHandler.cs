using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs.Host;
using ServiceA.Messages;

namespace ServiceA.Host.Handlers
{
    public class MessageExampleHandler : IFunction<MessageExample>
    {       
        public Task InvokeAsync(MessageExample input, ITraceWriter Log)
        {
            Log.Info("function called");

            return Task.CompletedTask;
        }

      
    }
}
