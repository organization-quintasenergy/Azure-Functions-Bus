using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace UIExample
{
    public class Events : Hub
    {      

        public async Task Send(string message)
        {
            await Clients.All.SendAsync("Send", $"{Context.ConnectionId}: {message}");
        }
                       
    }
}
