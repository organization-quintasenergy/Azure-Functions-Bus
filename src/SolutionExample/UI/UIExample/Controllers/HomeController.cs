using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AFBus;
using Microsoft.AspNetCore.Mvc;
using OrderSaga.Messages;
using UIExample.Models;

namespace UIExample.Controllers
{
    public class CardModel
    {
        [Required]
        public string User { get; set; }
                
        public string Product { get; set; }
    }

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            
            return View();
        }

        [HttpPost]
        public IActionResult AddToCard(CardModel model)
        {
            SendOnlyBus.SendAsync(new CartItemAdded() { UserName = model.User, ProductName = model.Product }, "ordersaga").Wait();

            return View("Index");
        }

        [HttpPost]
        public IActionResult ProcessOrder(CardModel model)
        {
            SendOnlyBus.SendAsync(new ProcessOrder() { UserName = model.User }, "ordersaga").Wait();

            return View("Index");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
