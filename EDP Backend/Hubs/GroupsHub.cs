using Microsoft.AspNetCore.SignalR;

namespace EDP_Backend.Hubs
{

    public class GroupsHub : Hub
    {
        public async Task Register(int id)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Connected " + id);
        }
    }
}
