---
title: "Creating MCP Servers in .NET"
category: "ASP.NET Core"
date: "June 3, 2026"
readTime: "12 min read"
excerpt: "Build Model Context Protocol (MCP) servers in .NET using both the native project template and integration with existing Web APIs."
tags: ["MCP", "ASP.NET Core", "Claude", "AI Integration"]
sidebar:
  - href: "#what-is-mcp"
    text: "What is MCP"
  - href: "#native-mcp-server-template"
    text: "Native MCP Template"
  - href: "#mcp-in-existing-web-api"
    text: "MCP in Web API"
  - href: "#resources-and-tools"
    text: "Resources and Tools"
  - href: "#best-practices"
    text: "Best Practices"
---

The Model Context Protocol (MCP) enables LLMs like Claude to safely access external data and tools through a standardized interface. .NET provides excellent tooling for building MCP servers, either as standalone services or integrated into existing Web APIs.

## What is MCP

**MCP (Model Context Protocol)** is an open protocol that allows large language models to interact with external resources. Think of it as a bridge between AI Agents and your application's data, APIs and tools.

### Why Use MCP in .NET?

- **Type Safety**: Leverage C# type system for schemas and tools
- **Performance**: Fast serialization with System.Text.Json
- **Integration**: Seamlessly connect with existing .NET services
- **Scalability**: Use with your existing ASP.NET Core infrastructure

## Native MCP Server Template

The fastest way to start is using the official .NET MCP Server template.

### Creating a New MCP Server

```bash
# Install the latest template
dotnet new install Microsoft.McpServer.ProjectTemplates

# Create a new MCP server
dotnet new mcpserver -n SampleMcpServer
cd SampleMcpServer

# Run the server (listens on stdio)
dotnet run
```

### Project Structure

```
SampleMcpServer/
├── .mcp/                # Define your resources here
├── Tools/               # Define your tools here
├── Program.cs           # Server initialization
└── SampleMcpServer.csproj
```

### Basic Server Setup

```csharp
// Program.cs
using Huki.Acturis.Modular;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SampleMcpServer.Tools.SampleTool>();

await builder.Build().RunAsync();
```

### Adding Tools

Tools let agents perform actions:

```csharp
// Tools/CalculatorTool.cs
using ModelContextProtocol.Server;

namespace SampleMcpServer.Tools;

internal sealed class CalculatorTool
{
    [McpServerTool]
    [Description("Adds two numbers.")]
    public async Task<object> Calculate(
        [Description("First number.")]
        int firstNumber = 1,
        [Description("Second number.")]
        int secondNumber = 2,
        CancellationToken cancellationToken = default)
    {
        return new
        {
            result = first + second,
        };
    }
}
```

## MCP in Existing Web API

Sometimes you want to add MCP capabilities to an existing ASP.NET Core Web API rather than create a separate service.

### Setup

```bash
dotnet add package ModelContextProtocol
```

### Configuring MCP with Dependency Injection

```csharp
// Program.cs
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SampleMcpServer.Tools.SampleTool>();

var app = builder.Build();

// Regular ASP.NET Core endpoints continue to work
app.MapGet("/api/data", () => "Your normal API");

await app.RunAsync();
```

### Accessing Services from MCP Tools

MCP tools can access your application's services:

```csharp
// Tools/CalculatorTool.cs
using ModelContextProtocol.Server;

namespace SampleMcpServer.Tools;

internal sealed class CalculatorTool
{
    private readonly ICalculatorService _calculatorService;
    
    public CalculatorTool(ICalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }
    
    [McpServerTool]
    [Description("Adds two numbers.")]
    public async Task<object> Calculate(
        [Description("First number.")]
        int firstNumber = 1,
        [Description("Second number.")]
        int secondNumber = 2,
        CancellationToken cancellationToken = default)
    {
        return new
        {
            result = calculatorService.Calculate(first, second),
        };
    }
}
```

## Conclusion

MCP servers unlock powerful integrations between AI Agents and your .NET applications. Whether you choose a standalone native server or integrate into an existing API, the .NET ecosystem provides excellent tooling and type safety.

Start with the native template for new projects, or add MCP capabilities incrementally to existing APIs using dependency injection. Always prioritize security and validate all inputs from the LLM.

- Use the native template for dedicated MCP services
- Integrate MCP into Web APIs for unified deployments
- Leverage dependency injection for clean architecture
- Always validate and sanitize inputs from AI Agents
- Monitor and log all MCP interactions in production
