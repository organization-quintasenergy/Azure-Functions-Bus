using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace UIExample
{
    public class Events : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Send", $"{Context.ConnectionId} joined");
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.SendAsync("Send", $"{Context.ConnectionId} left");
        }

        public async Task Send(string message)
        {
            await Clients.All.SendAsync("Send", $"{Context.ConnectionId}: {message}");
        }

        
       
    }
}
