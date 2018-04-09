using OrderSaga.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UIExample.ViewModels
{
    public class CartViewModel
    {       

        public string Product { get; set; }

        public List<CartItemAdded> CartItemsAdded { get; set; }

    }
}
