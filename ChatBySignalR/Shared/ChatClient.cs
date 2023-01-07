using Microsoft.AspNetCore.SignalR.Client;

namespace ChatBySignalR.Shared;

public class ChatClient : IAsyncDisposable
{
    public const string SignalRHubUrl = "/ChatHub";

    private readonly string _hubUrl;
    private readonly string _username;

    private HubConnection _hubConnection;
    private bool _started = false;

    public ChatClient(string username, string siteUrl)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
        if (string.IsNullOrWhiteSpace(siteUrl)) throw new ArgumentNullException(nameof(siteUrl));

        _username = username;
        _hubUrl = siteUrl.TrimEnd('/') + SignalRHubUrl;
    }

    public async Task StartAsync()
    {
        if (!_started)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect()
                .Build();


            // add handler for receiving messages
            _hubConnection.On<string, string>(Messages.ReceiveMessageEvent, (user, message) =>
             {
                 HandleReceiveMessage(user, message);
             });

            await _hubConnection.StartAsync();

            Console.WriteLine("Chat client - connection started for user", _username);
            _started = true;
            await _hubConnection.SendAsync(Messages.RegisterUser, _username);
        }
    }

    /// <summary>
    /// Handle an inbound message from a hub
    /// </summary>
    /// <param name="method">event name</param>
    /// <param name="message">message content</param>
    private void HandleReceiveMessage(string username, string message)
    {
        // raise an event to subscribers
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(username, message));
    }

    /// <summary>
    /// Event raised when this client receives a message
    /// </summary>
    /// <remarks>
    /// Instance classes should subscribe to this event
    /// </remarks>
    public event MessageReceivedEventHandler MessageReceived;

    /// <summary>
    /// Send a message to the hub
    /// </summary>
    /// <param name="message">message to send</param>
    public async Task SendAsync(string message)
    {
        // check we are connected
        if (!_started)
            throw new InvalidOperationException("Client not started");
        // send the message
        await _hubConnection.SendAsync(Messages.SendMessage, _username, message);
    }

    /// <summary>
    /// Stop the client (if started)
    /// </summary>
    public async Task StopAsync()
    {
        if (_started)
        {
            // disconnect the client
            await _hubConnection.StopAsync();
            // There is a bug in the mono/SignalR client that does not
            // close connections even after stop/dispose
            // see https://github.com/mono/mono/issues/18628
            // this means the demo won't show "xxx left the chat" since 
            // the connections are left open
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            _started = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("ChatClient: Disposing");
        await StopAsync();
    }
}

/// <summary>
/// Delegate for the message handler
/// </summary>
/// <param name="sender">the SignalRclient instance</param>
/// <param name="e">Event args</param>
public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

