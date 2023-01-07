
using ChatBySignalR.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBySignalR.Server.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> userLookup = new Dictionary<string, string>();

    public async Task SendMessage(string username, string message)
    {
        await Clients.All.SendAsync(Messages.ReceiveMessageEvent, username, message);
    }

    public async Task Register(string username)
    {
        var currentId = Context.ConnectionId;
        if (!userLookup.ContainsKey(currentId))
        {
            userLookup.Add(currentId, username);
            // re-use existing message for now
            await Clients.AllExcept(currentId).SendAsync(
                Messages.ReceiveMessageEvent,
                username, $"{username} dołączył do chatu");
        }
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine("Connected");
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception e)
    {
        // try to get connection
        string id = Context.ConnectionId;
        if (!userLookup.TryGetValue(id, out string username))
            username = "unknown user";

        userLookup[id] = $"[{userLookup[id]}]";
        await Clients.AllExcept(Context.ConnectionId).SendAsync(
            Messages.ReceiveMessageEvent,
            username, $"{username} has left the chat");
        await base.OnDisconnectedAsync(e);
    }


}
