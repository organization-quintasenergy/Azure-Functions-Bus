using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService
{
    public interface IPaymentsRepository
    {
        void AddOrderPayed(OrderPayed o);


        List<OrderPayed> GetOrdersPayed();
       
    }

    public  class InMemoryPaymentsRepository : IPaymentsRepository
    {
        static List<OrderPayed> ordersPayed = new List<OrderPayed>();

        public void AddOrderPayed(OrderPayed o)
        {
            ordersPayed.Add(o);
        }

        public List<OrderPayed> GetOrdersPayed()
        {
            return ordersPayed;
        }
    }

    public class OrderPayed
    {
        public string User { get; set; }
    }
    
}
