using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService
{
    internal class InMemoryPaymentsDataBase
    {
        static List<OrderPayed> ordersPayed = new List<OrderPayed>();

        internal void AddOrderPayed(OrderPayed o)
        {
            ordersPayed.Add(o);
        }

        internal List<OrderPayed> GetOrdersPayed()
        {
            return ordersPayed;
        }
    }

    class OrderPayed
    {
        public string UserName { get; set; }
    }
    
}
