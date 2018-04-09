using AFBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSaga.Sagas
{
    public class OrderSagaData : SagaData
    {
        public string UserName { get; set; }

        public string Products { get; set; }

        public bool Shipped { get; set; }

        public bool Payed { get; set; }
    }
}
