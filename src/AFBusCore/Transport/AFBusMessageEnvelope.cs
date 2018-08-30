using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class AFBusMessageContext
    {
        public Guid MessageID { get; set; }

        public Guid? TransactionID { get; set; }
                      
        public TimeSpan? MessageDelayedTime { get; set; }

        public DateTime? MessageFinalWakeUpTimeStamp { get; set; }

        public string BodyType { get; set; }

        public string Destination { get; set; }

        public bool BodyInFile { get; set; }

        public string SenderServiceName { get; set; }

        public string ActualServiceName { get; set; }
    }

    public class AFBusMessageEnvelope
    {
        public AFBusMessageContext Context { get; set; }

        public string Body { get; set; }
    }
}
