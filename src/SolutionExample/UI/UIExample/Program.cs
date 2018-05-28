using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AFBus;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UIExample
{
    public class Program
    {        

        public static void Main(string[] args)
        {
            var wh = BuildWebHost(args);

            wh.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .Build();
    }
}
