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

namespace UIExample.Controllers
{
    
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            IPaymentProxies paymentProxies = new PaymentProxies();

            var result = await paymentProxies.GetPayments();

            return View();
        }

        [HttpPost]
        public IActionResult AddToCard(CartViewModel model)
        {
            SendOnlyBus.SendAsync(new CartItemAdded() { UserName = model.User, ProductName = model.Product }, "ordersaga").Wait();

            return View("Index");
        }

        [HttpPost]
        public IActionResult ProcessOrder(CartViewModel model)
        {
            SendOnlyBus.SendAsync(new ProcessOrder() { UserName = model.User }, "ordersaga").Wait();

            return View("Index");
        }

        
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
