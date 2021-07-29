using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub: Hub
    {
        private readonly PresenceTracker _presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }
        
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOnline) 
                await this.Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var users = await _presenceTracker.GetOnlineUsers();
            await this.Clients.Caller.SendAsync("GetOnlineUsers", users);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var isOffline = await _presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOffline) 
                await this.Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            await base.OnDisconnectedAsync(exception);
        }
    }
}