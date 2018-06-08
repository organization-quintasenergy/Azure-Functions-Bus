using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface ISendMessages
    {
        /// <summary>
        /// Returns the max time the transport can delay a message
        /// </summary>
        /// <returns></returns>
        TimeSpan MaxDelay();

        int MaxMessageSize();
        
        Task SendMessageAsync<T>(T message, string serviceName, AFBusMessageContext context) where T : class;

        Task<string> ReadMessageBodyFromFileAsync(string fileName);

        Task DeleteFileWithMessageBodyAsync(string fileName);
    }
}
