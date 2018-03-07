using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public abstract class Saga<T> where T: SagaData,new()
    {
        public Saga()
        {
            Data = new T();
            
        }

        public T Data { get; set; }

        
    }

    
}
