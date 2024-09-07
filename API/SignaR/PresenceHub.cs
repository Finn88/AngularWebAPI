using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignaR
{
  [Authorize]
  public class PresenceHub(PresenceTracker presenceTracker) : Hub
  {
    public override async Task OnConnectedAsync()
    {
      if (Context.User == null) throw new HubException();

      var isOnline = await presenceTracker.UserConnected(Context.User?.GetUserName()!, Context.ConnectionId);
      if (isOnline) await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUserName());

      var currentUsers = await presenceTracker.GetOnlineUsers();
      await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }
    public override async Task OnDisconnectedAsync(Exception? ex)
    {
      if (Context.User == null) throw new HubException();

      var isOffline = await presenceTracker.UserDisconnected(Context.User?.GetUserName()!, Context.ConnectionId);
      if (isOffline) await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUserName());

      await base.OnDisconnectedAsync(ex);
    }
  }
}
