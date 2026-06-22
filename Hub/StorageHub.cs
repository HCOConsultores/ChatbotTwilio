

namespace chatBotTwilio.Hub
{
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.SignalR;
    
    public class StorageHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task TriggerUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveUpdate", message);
        }
    }
}