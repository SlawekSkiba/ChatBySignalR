using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using ChatBySignalR.Client;
using ChatBySignalR.Client.Shared;
using ChatBySignalR.Shared;
using MudBlazor;

namespace ChatBySignalR.Client.Pages
{
    public partial class Index
    {
        bool chatting;
        bool ShowDialog { get => !chatting; set => chatting = !value; }
        string username = "";
        private DialogOptions dialogOptions = new() { FullWidth = true };

        ChatClient? client = null;

        string? notification = null;
        string? message = null;

        List<Message> messages = new List<Message>();

#pragma warning disable CS8618 
        [Inject]
        NavigationManager NavigationManager { get; set; }
#pragma warning restore CS8618 

        /// <summary>
        /// Start chat client
        /// </summary>
        async Task Chat()
        {
            // check username is valid
            if (string.IsNullOrWhiteSpace(username))
            {
                notification = "Please enter a name";
                return;
            }

            ;
            try
            {
                // remove old messages if any
                messages.Clear();
                // Create the chat client
                string baseUrl = NavigationManager.BaseUri;
                client = new ChatClient(username, baseUrl);
                // add an event handler for incoming messages
                client.MessageReceived += MessageReceived;
                // start the client
                Console.WriteLine("Index: chart starting...");
                await client.StartAsync();
                Console.WriteLine("Index: chart started?");
                chatting = true;
            }
            catch (Exception e)
            {
                notification = $"ERROR: Failed to start chat client: {e.Message}";
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Inbound message
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine($"Blazor: receive {e.Username}: {e.Message}");
            bool isMine = false;
            if (!string.IsNullOrWhiteSpace(e.Username))
            {
                isMine = string.Equals(e.Username, username, StringComparison.CurrentCultureIgnoreCase);
            }

            var newMsg = new Message(e.Username, e.Message, isMine);
            messages.Add(newMsg);
            // Inform blazor the UI needs updating
            StateHasChanged();
        }

        async Task DisconnectAsync()
        {
            if (chatting)
            {
                await client.StopAsync();
                client = null;
                notification = "chat ended";
                chatting = false;
            }
        }

        async Task SendAsync()
        {
            if (chatting && !string.IsNullOrWhiteSpace(message))
            {
                // send message to hub
                await client.SendAsync(message);
                // clear input box
                message = "";
            }
        }

        class Message
        {
            public Message(string username, string body, bool mine)
            {
                Username = username;
                Body = body;
                Mine = mine;
            }

            public string Username { get; set; }

            public string Body { get; set; }

            public bool Mine { get; set; }

            /// <summary>
            /// Determine CSS classes to use for message div
            /// </summary>
            public string CSS
            {
                get
                {
                    return Mine ? "sent" : "received";
                }
            }
        }
    }
}