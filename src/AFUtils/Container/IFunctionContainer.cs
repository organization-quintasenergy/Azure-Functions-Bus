using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace AFUtils.IoC
{
    public interface IFunctionContainer 
    {
        
        Task InvokeAsync<T>(T message, ITraceWriter log);

    }
}
