using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AFBus;
using Microsoft.AspNetCore.Mvc;
using OrderSaga.Messages;
using UIExample.Proxies;
using UIExample.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace UIExample.Controllers
{
    
    public class HomeController : Controller
    {
        private const string CARDKEY = "CARDINSESSION";
        private const string ORDERSAGASERVICENAME = "ordersaga";
        private const string USER = "USER";

        public async Task<IActionResult> Index()
        {
            IPaymentProxies paymentProxies = new PaymentProxies();

            var result = await paymentProxies.GetPayments();

            var cartViewModel = new CartViewModel();

            var cartItems = GetCartInSession();

            cartViewModel.CartItemsAdded = cartItems;

            return View(cartViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCard(CartViewModel cartViewModel)
        {           
            var cartItemAdded = new CartItemAdded() { UserName = USER, ProductName = cartViewModel.Product };

            await SendOnlyBus.SendAsync(cartItemAdded, ORDERSAGASERVICENAME);

            AddItemsToCartInSession(cartItemAdded);

            var cartItems = GetCartInSession();

            cartViewModel.CartItemsAdded = cartItems;

            return View("Index", cartViewModel);
        }
               

        [HttpPost]
        public async Task<IActionResult> ProcessOrder()
        {
            var cartItems = GetCartInSession();
            var cartViewModel = new CartViewModel();

            cartViewModel.CartItemsAdded = cartItems;

            await SendOnlyBus.SendAsync(new ProcessOrder() { UserName = USER }, ORDERSAGASERVICENAME);

            return View("Index", cartViewModel);
        }

        
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private List<CartItemAdded> GetCartInSession()
        {
            var value = HttpContext.Session.GetString(CARDKEY);

            return value == null ? new List<CartItemAdded>() : JsonConvert.DeserializeObject<List<CartItemAdded>>(value);
        }

        private void ClearCartInSession()
        {
            HttpContext.Session.Remove(CARDKEY);

            
        }

        private void AddItemsToCartInSession(CartItemAdded item)
        {
            var cart = GetCartInSession();

            cart.Add(item);

            HttpContext.Session.SetString(CARDKEY, JsonConvert.SerializeObject(cart));
        }
    }
}
