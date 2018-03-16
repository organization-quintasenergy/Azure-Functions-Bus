using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AFBus;
using Microsoft.AspNetCore.Mvc;
using OrderSaga.Messages;
using UIExample.Models;

namespace UIExample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            SendOnlyBus.SendAsync(new CartItemAdded() { UserName = "pablo", ProductName = "raspberry pi" }, "ordersaga").Wait();

            return View();
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
