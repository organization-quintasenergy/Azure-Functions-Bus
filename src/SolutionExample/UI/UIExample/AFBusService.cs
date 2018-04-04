using AFBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UIExample
{
    public class AFBusService : IHostedService
    {
        public static HandlersContainer container = new HandlersContainer();

        IHubContext<Events> hubcontext;

        static AFBusService()
        {
            
        }

        public AFBusService(IHubContext<Events> hubcontext)
        {
            this.hubcontext = hubcontext;

            HandlersContainer.AddDependencyWithInstance<IHubContext<Events>>(hubcontext);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
