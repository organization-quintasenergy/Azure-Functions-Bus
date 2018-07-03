using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandlersContainer 
    {
        
        Task HandleCommandAsync<T>(T message, TraceWriter log) where T : class;

        Task HandleCommandAsync(string serializedMessage, TraceWriter log);

    }
}
