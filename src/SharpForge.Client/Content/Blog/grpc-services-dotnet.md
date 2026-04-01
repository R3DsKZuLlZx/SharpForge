---
title: "gRPC Services in .NET"
category: "ASP.NET Core"
date: "October 25, 2025"
readTime: "10 min read"
excerpt: "Build high-performance microservices with gRPC. Covers protobuf, streaming, and integration with ASP.NET Core."
tags: ["gRPC", "Microservices", "Protocol Buffers", "ASP.NET Core"]
sidebar:
  - href: "#getting-started"
    text: "Getting Started"
  - href: "#defining-services-with-protobuf"
    text: "Defining Services"
  - href: "#implementing-the-service"
    text: "Implementation"
  - href: "#creating-a-client"
    text: "Creating Clients"
  - href: "#streaming-patterns"
    text: "Streaming Patterns"
  - href: "#interceptors"
    text: "Interceptors"
---

gRPC is a high-performance RPC framework that uses Protocol Buffers for serialization. It's ideal for microservices communication.

## Getting Started

```bash
# Create a gRPC service
dotnet new grpc -n MyGrpcService

# Required packages (already included in template)
dotnet add package Grpc.AspNetCore
```

## Defining Services with Protobuf

```protobuf
// Protos/greet.proto
syntax = "proto3";

option csharp_namespace = "MyGrpcService";

package greet;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
  rpc SayHelloStream (HelloRequest) returns (stream HelloReply);
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
  int32 count = 2;
}
```

### Project Configuration

```xml
<!-- .csproj -->
<ItemGroup>
  <Protobuf Include="Protos\*.proto" GrpcServices="Server" />
</ItemGroup>

<!-- For client -->
<ItemGroup>
  <Protobuf Include="Protos\*.proto" GrpcServices="Client" />
</ItemGroup>
```

## Implementing the Service

```csharp
public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(
        HelloRequest request, 
        ServerCallContext context)
    {
        _logger.LogInformation("Greeting {Name}", request.Name);
        
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello, {request.Name}!",
            Count = 1
        });
    }

    public override async Task SayHelloStream(
        HelloRequest request,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        for (int i = 0; i < 5; i++)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            await responseStream.WriteAsync(new HelloReply
            {
                Message = $"Hello #{i + 1}, {request.Name}!",
                Count = i + 1
            });

            await Task.Delay(1000);
        }
    }
}
```

## Server Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
    options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
});

var app = builder.Build();

app.MapGrpcService<GreeterService>();

app.Run();
```

## Creating a Client

```csharp
// Console client
using var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new Greeter.GreeterClient(channel);

// Unary call
var reply = await client.SayHelloAsync(new HelloRequest { Name = "World" });
Console.WriteLine(reply.Message);

// Server streaming
using var streamingCall = client.SayHelloStream(new HelloRequest { Name = "World" });

await foreach (var response in streamingCall.ResponseStream.ReadAllAsync())
{
    Console.WriteLine(response.Message);
}
```

### Client with DI

```csharp
// Register in Program.cs
builder.Services.AddGrpcClient<Greeter.GreeterClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Inject in service
public class MyService
{
    private readonly Greeter.GreeterClient _client;

    public MyService(Greeter.GreeterClient client)
    {
        _client = client;
    }

    public async Task<string> GreetAsync(string name)
    {
        var reply = await _client.SayHelloAsync(new HelloRequest { Name = name });
        return reply.Message;
    }
}
```

## Streaming Patterns

### Client Streaming

```protobuf
rpc SendMessages (stream ChatMessage) returns (ChatSummary);
```

```csharp
// Implementation
public override async Task<ChatSummary> SendMessages(
    IAsyncStreamReader<ChatMessage> requestStream,
    ServerCallContext context)
{
    var messages = new List<string>();
    
    await foreach (var message in requestStream.ReadAllAsync())
    {
        messages.Add(message.Content);
    }

    return new ChatSummary 
    { 
        MessageCount = messages.Count,
        Messages = { messages }
    };
}
```

### Bidirectional Streaming

```protobuf
rpc Chat (stream ChatMessage) returns (stream ChatMessage);
```

```csharp
// Implementation
public override async Task Chat(
    IAsyncStreamReader<ChatMessage> requestStream,
    IServerStreamWriter<ChatMessage> responseStream,
    ServerCallContext context)
{
    await foreach (var message in requestStream.ReadAllAsync())
    {
        // Echo back with modification
        await responseStream.WriteAsync(new ChatMessage
        {
            User = "Server",
            Content = $"Received: {message.Content}"
        });
    }
}
```

## Error Handling

```csharp
public override Task<GetProductReply> GetProduct(
    GetProductRequest request,
    ServerCallContext context)
{
    var product = _repository.GetById(request.Id);
    
    if (product is null)
    {
        throw new RpcException(new Status(
            StatusCode.NotFound,
            $"Product {request.Id} not found"));
    }

    return Task.FromResult(new GetProductReply { Product = product.ToProto() });
}

// Client handling
try
{
    var product = await client.GetProductAsync(new GetProductRequest { Id = 999 });
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
{
    Console.WriteLine($"Not found: {ex.Status.Detail}");
}
```

## Interceptors

```csharp
public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            return await continuation(request, context);
        }
        finally
        {
            _logger.LogInformation(
                "{Method} completed in {ElapsedMs}ms",
                context.Method,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

// Register
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<LoggingInterceptor>();
});
```

## Best Practices

- Use gRPC for service-to-service communication
- Consider gRPC-Web for browser clients
- Implement proper error handling with status codes
- Use interceptors for cross-cutting concerns
- Version your proto files carefully
- Use deadlines for all calls
