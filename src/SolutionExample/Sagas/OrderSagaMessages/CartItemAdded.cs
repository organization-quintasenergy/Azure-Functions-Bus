using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSaga.Messages
{
    public class CartItemAdded
    {

        public string UserName { get; set; }

        public string ProductName { get; set; }
    }
}
