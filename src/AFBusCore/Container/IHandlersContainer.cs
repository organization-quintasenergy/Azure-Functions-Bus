using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AFBus
{
    public interface IHandlersContainer 
    {
        
        Task HandleAsync<T>(T message, ILogger log) where T : class;

        Task HandleAsync(string serializedMessage, ILogger log);

    }
}
