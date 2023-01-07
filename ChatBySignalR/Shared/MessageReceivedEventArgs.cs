using System;

namespace ChatBySignalR.Shared
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }

        public string Username { get; set; }

        public string Message { get; set; }

    }

}

