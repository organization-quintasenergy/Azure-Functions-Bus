using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFUtils
{
    public interface ITraceWriter
    {
       
        TraceLevel Level { get; set; }

        void Error(string message, Exception ex = null, string source = null);
      
        void Flush();
       
        void Info(string message, string source = null);
       
               
        void Verbose(string message, string source = null);
      
        void Warning(string message, string source = null);
    }
}
