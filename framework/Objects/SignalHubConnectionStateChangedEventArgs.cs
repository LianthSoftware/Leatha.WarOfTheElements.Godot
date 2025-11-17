using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Leatha.WarOfTheElements.Godot.framework.Objects
{
    public sealed class SignalHubConnectionStateChangedEventArgs : EventArgs
    {
        public HubConnectionState State { get; set; }
    }
}
