using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    class SagaInfo
    {

        public Type SagaType { get; set; }

        public List<Type> MessagesThatAreHandledByTheSaga { get; set; }

        public List<Type> MessagesThatActivatesTheSaga { get; set; }
    }
}
