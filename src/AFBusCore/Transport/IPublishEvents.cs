using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IPublishEvents
    {
        /// <summary>
        /// Returns the max time the transport can delay a message
        /// </summary>
        /// <returns></returns>
        TimeSpan MaxDelay();

        int MaxMessageSize();

        Task PublishEventsAsync<T>(T message, string topicName, AFBusMessageContext context) where T : class;

        Task<string> ReadMessageBodyFromFileAsync(string fileName);

        Task DeleteFileWithMessageBodyAsync(string fileName);
    }
}
