using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingService
{
    public interface IShippingRepository
    {
        void AddOrderShipped(OrderShipped o);


        List<OrderShipped> GetOrdersShipped();

    }

    public class InMemoryShippingRepository : IShippingRepository
    {
        static List<OrderShipped> ordershipped = new List<OrderShipped>();

        public void AddOrderShipped(OrderShipped o)
        {
            ordershipped.Add(o);
        }

        public List<OrderShipped> GetOrdersShipped()
        {
            return ordershipped;
        }
    }

    public class OrderShipped
    {
        public string User { get; set; }

        public string Address { get; set; }
    }
}


