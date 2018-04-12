using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AFBus.Tests
{
    class AzureStorageQueueSendTransportShortMaxDelay : AzureStorageQueueSendTransport
    {
        ISerializeMessages serializer;

        private static HashSet<string> createdQueues = new HashSet<string>();

        public AzureStorageQueueSendTransportShortMaxDelay(ISerializeMessages serializer):base(serializer)
        {
            this.serializer = serializer;
        }

       
        public override TimeSpan MaxDelay()
        {
            return TimeSpan.FromSeconds(5);
        }
    }
}
