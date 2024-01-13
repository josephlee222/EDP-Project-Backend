using Microsoft.AspNetCore.SignalR;

namespace EDP_Backend.Hubs
{

    public class ActionsHub : Hub
    {
     

        public async Task Register(int id)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Connected");
        }

        // function to send a action request to a specific user
        public async Task SendActionToUser(int id, string action)
        {
            await Clients.Group(id.ToString()).SendAsync("ReceiveAction", action);
        }

        public async Task SendAction(string action)
        {
            await Clients.All.SendAsync("ReceiveAction", action);
        }
    }
}
