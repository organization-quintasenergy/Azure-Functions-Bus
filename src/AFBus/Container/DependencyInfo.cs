using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    class DependencyInfo
    {
        public Type Interface { get; set; }

        public Type ConcreteType { get; set; }

        public object[] args { get; set; }

        public object instance { get; set; }
    }
}
