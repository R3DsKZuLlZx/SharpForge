---
title: "Building Real-Time Apps with Blazor and SignalR"
category: "Blazor"
date: "January 22, 2026"
readTime: "10 min read"
excerpt: "Learn how to create interactive real-time applications using Blazor WebAssembly and SignalR for seamless client-server communication."
tags: ["Blazor", "SignalR", "Real-Time", "WebAssembly"]
sidebar:
  - href: "#what-is-signalr"
    text: "What is SignalR?"
  - href: "#setting-up-the-project"
    text: "Setting Up the Project"
  - href: "#building-the-client"
    text: "Building the Client"
  - href: "#advanced-features"
    text: "Advanced Features"
  - href: "#best-practices"
    text: "Best Practices"
  - href: "#conclusion"
    text: "Conclusion"
---

Real-time functionality has become a staple of modern web applications. From chat applications to live dashboards, users expect instant updates without refreshing the page. In this tutorial, we'll explore how to build real-time features using Blazor WebAssembly and SignalR.

## What is SignalR?

SignalR is a library that simplifies adding real-time web functionality to applications. It enables server-side code to push content to connected clients instantly. SignalR handles connection management automatically and can scale to thousands of simultaneous connections.

## Setting Up the Project

Let's start by creating a new Blazor WebAssembly project with a SignalR hub.

### 1. Create the Solution

```bash
dotnet new blazorwasm -ho -n RealTimeApp
cd RealTimeApp
```

### 2. Add SignalR to the Server

In the Server project, add the SignalR services:

```csharp
// Program.cs (Server)
builder.Services.AddSignalR();

// After app.UseRouting()
app.MapHub<ChatHub>("/chathub");
```

### 3. Create the Hub

```csharp
// Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.ConnectionId);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
```

## Building the Client

Now let's create a Blazor component that connects to our SignalR hub.

### Install the Client Package

```bash
cd Client
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

### Create the Chat Component

```razor
@page "/chat"
@using Microsoft.AspNetCore.SignalR.Client
@inject NavigationManager Navigation
@implements IAsyncDisposable

<div class="chat-container">
    <div class="messages">
        @foreach (var message in messages)
        {
            <div class="message">
                <strong>@message.User:</strong> @message.Text
            </div>
        }
    </div>

    <div class="input-area">
        <input @bind="userName" placeholder="Your name" />
        <input @bind="messageText" placeholder="Type a message..." />
        <button @onclick="SendMessage" disabled="@(!IsConnected)">
            Send
        </button>
    </div>

    <p>Status: @(IsConnected ? "Connected" : "Disconnected")</p>
</div>

@code {
    private HubConnection? hubConnection;
    private List<ChatMessage> messages = new();
    private string userName = "";
    private string messageText = "";

    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            messages.Add(new ChatMessage(user, message));
            InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
    }

    private async Task SendMessage()
    {
        if (hubConnection is not null && !string.IsNullOrEmpty(messageText))
        {
            await hubConnection.SendAsync("SendMessage", userName, messageText);
            messageText = "";
        }
    }

    private bool IsConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    private record ChatMessage(string User, string Text);
}
```

## Advanced Features

### Strongly-Typed Hubs

For better type safety, you can create strongly-typed hubs:

```csharp
// Shared interface (in a shared project)
public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
    Task UserConnected(string connectionId);
    Task UserDisconnected(string connectionId);
}

// Strongly-typed hub
public class ChatHub : Hub<IChatClient>
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }
}
```

### Connection State Management

Handle connection state changes gracefully:

```csharp
hubConnection.Reconnecting += error =>
{
    connectionStatus = "Reconnecting...";
    InvokeAsync(StateHasChanged);
    return Task.CompletedTask;
};

hubConnection.Reconnected += connectionId =>
{
    connectionStatus = "Connected";
    InvokeAsync(StateHasChanged);
    return Task.CompletedTask;
};

hubConnection.Closed += error =>
{
    connectionStatus = "Disconnected";
    InvokeAsync(StateHasChanged);
    return Task.CompletedTask;
};
```

## Best Practices

- **Always use automatic reconnection** - Network issues are common, and SignalR's built-in reconnection handles them gracefully
- **Dispose connections properly** - Implement IAsyncDisposable and clean up hub connections
- **Use groups for targeted messaging** - Instead of broadcasting to all clients, use groups to send messages to specific subsets
- **Consider using strongly-typed hubs** - They provide compile-time safety and better IntelliSense
- **Handle disconnections** - Users may lose connection; always provide feedback about connection state

## Conclusion

Blazor and SignalR make an excellent combination for building real-time web applications. With minimal setup, you can create interactive experiences that update instantly across all connected clients. Whether you're building a chat application, a live dashboard, or a collaborative tool, these technologies provide the foundation you need.
