using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IBus
    {
        /// <summary>
        /// Sends a message to a queue named like the service.
        /// </summary>
        Task SendAsync<T>(T input, string serviceName);
    }
}
