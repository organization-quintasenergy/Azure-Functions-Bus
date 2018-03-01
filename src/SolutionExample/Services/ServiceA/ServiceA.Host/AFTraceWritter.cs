using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs.Host;

namespace ServiceA.Host
{
    public class AFTraceWritter : ITraceWriter
    {
        TraceWriter log;

        public AFTraceWritter(TraceWriter log)
        {
            this.log = log;
        }

        public TraceLevel Level { get => log.Level; set => log.Level=value; }

        public void Error(string message, Exception ex = null, string source = null)
        {
            log.Error(message, ex, source);
        }

        public void Flush()
        {
            log.Flush();
        }

        public void Info(string message, string source = null)
        {
            log.Info(message, source);
        }

        public void Verbose(string message, string source = null)
        {
            log.Verbose(message, source);
        }

        public void Warning(string message, string source = null)
        {
            log.Warning(message, source);
        }
    }
}
