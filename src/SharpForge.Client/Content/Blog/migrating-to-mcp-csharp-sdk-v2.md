---
title: "Migrating to MCP C# SDK v2.0"
category: "AI"
date: "August 4, 2026"
readTime: "12 min read"
excerpt: "The official MCP C# SDK hit 2.0. Your server almost certainly still compiles — what breaks is your package graph, anything experimental you leaned on, and the transport defaults underneath you."
tags: ["MCP", "AI Integration", "Migration", "ASP.NET Core"]
sidebar:
  - href: "#what-actually-broke"
    text: "What Actually Broke"
  - href: "#pin-the-version-first"
    text: "Pin the Version First"
  - href: "#the-tasks-api-is-gone"
    text: "The Tasks API Is Gone"
  - href: "#sampling-roots-and-logging-are-deprecated"
    text: "Deprecated Features"
  - href: "#stateful-http-is-now-the-escape-hatch"
    text: "Stateful HTTP"
  - href: "#what-you-get-in-return"
    text: "What You Get"
  - href: "#is-it-worth-upgrading"
    text: "Is It Worth It?"
  - href: "#conclusion"
    text: "Conclusion"
---

Version 2.0.0 of the official Model Context Protocol C# SDK is on NuGet, and 1.4.1 is the end of the v1 line. If you built an MCP server in .NET any time in the last year and never pinned a version, your next `dotnet restore` moved you across a major boundary. The good news is that this is a much gentler major than the version number suggests. The bad news is that the parts which do break are the parts nobody notices until production.

## What Actually Broke

Start with what did **not** break, because it is most of what you wrote. Building a server against 2.0.0, this compiles unchanged:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IShipmentTracker, ShipmentTracker>();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation { Name = "logistics-mcp", Version = "2.0.0" };
    })
    .WithHttpTransport()
    .WithTools<ShipmentTools>();

var app = builder.Build();
app.MapMcp("/mcp");
app.Run();
```

`AddMcpServer`, `WithStdioServerTransport`, `WithHttpTransport`, `WithTools<T>`, `WithToolsFromAssembly`, `MapMcp`, `[McpServerToolType]` and `[McpServerTool]` all survive with the same signatures. Tool classes with constructor-injected dependencies still work exactly as before:

```csharp
[McpServerToolType]
internal sealed class ShipmentTools(IShipmentTracker tracker)
{
    [McpServerTool(Name = "get_shipment_status", ReadOnly = true, Idempotent = true)]
    [Description("Returns the current status of a shipment.")]
    public async Task<string> GetShipmentStatusAsync(
        [Description("Carrier tracking number.")] string trackingNumber,
        CancellationToken cancellationToken)
        => await tracker.GetStatusAsync(trackingNumber, cancellationToken);
}
```

The breakage sits in three buckets, in descending order of how much it will hurt:

1. **The experimental tasks API is deleted.** Not deprecated — gone. Twenty-six public types removed.
2. **A large deprecation wave.** Sampling, roots and logging are all obsolete, as is stateful Streamable HTTP. These are warnings, unless you build with warnings as errors — and you should.
3. **The package graph is now rigid.** Mixed 1.x/2.x references no longer resolve.

## Pin the Version First

The v1-era instruction was `dotnet add package ModelContextProtocol`, unpinned. Do not do that again. Pin explicitly:

```bash
dotnet add package ModelContextProtocol.AspNetCore --version 2.0.0
```

This matters more in 2.0 than it did in 1.x because the dependency has gone from a floating minimum to an exact pin. In 1.4.1 the meta-package asked for `ModelContextProtocol.Core` version `1.4.1` — a minimum, satisfiable by anything newer. In 2.0.0 it asks for `[2.0.0]`, an exact-version bracket:

```xml
<!-- ✅ GOOD: one version for the whole family, pinned -->
<ItemGroup>
  <PackageReference Include="ModelContextProtocol.AspNetCore" Version="2.0.0" />
</ItemGroup>

<!-- ❌ BAD: a stale explicit Core reference now fails to resolve -->
<ItemGroup>
  <PackageReference Include="ModelContextProtocol.AspNetCore" Version="2.0.0" />
  <PackageReference Include="ModelContextProtocol.Core" Version="1.4.1" />
</ItemGroup>
```

If your solution references `ModelContextProtocol.Core` directly — common in projects that split protocol models into a shared library — you must move every reference in lockstep. There is no partial upgrade.

## The Tasks API Is Gone

The largest single deletion is the experimental **tasks** feature: the long-running-tool model where a tool call returned a task id the client polled to completion. Removed outright:

```text
IMcpTaskStore                    McpTask                 McpTaskStatus
InMemoryMcpTaskStore             McpTaskMetadata         McpTasksCapability
CreateTaskResult                 GetTaskResult           ListTasksResult
CancelMcpTaskRequestParams       ToolExecution           ToolTaskSupport
...and the full *McpTasksCapability family
```

Along with the members that hung off them: `McpServerOptions.TaskStore`, `McpServerOptions.SendTaskStatusNotifications`, `Tool.Execution`, `McpClient.CallToolAsTaskAsync`, `McpClient.PollTaskUntilCompleteAsync`, `McpServer.GetTaskAsync` and friends.

Every one of these carried `[Experimental]` in v1, so you could not have used them without explicitly suppressing a diagnostic. That is the SDK's contract working as designed — but if your suppression is a blanket `<NoWarn>` in a `Directory.Build.props`, you had no warning that you were standing on sand. Search for it before you upgrade:

```bash
grep -rn "IMcpTaskStore\|CallToolAsTaskAsync\|ToolTaskSupport" --include=*.cs .
```

The replacement is a genuine re-model rather than a rename, built around input requests: `InputRequest`, `InputResponse`, `InputRequiredResult` and `InputRequiredException`, with `McpClient.ResolveInputRequestsAsync` on the client side. A tool that needs something from the caller mid-flight now throws rather than parking a task:

```csharp
throw new InputRequiredException(
    new Dictionary<string, InputRequest>
    {
        ["confirmation"] = InputRequest.ForElicitation(
            new ElicitRequestParams { Message = "Write off shipment ABC123?" })
    },
    requestState: "write-off-pending");
```

The server also exposes `McpServer.IsMrtrSupported` so you can branch on whether the connected client can take part in a multi-round exchange at all. If you were using tasks in anger, budget real time for this — it is a rewrite of that code path, not a find-and-replace.

## Sampling, Roots and Logging Are Deprecated

This is the change most likely to light up your build. Three long-standing features are now obsolete under diagnostic **MCP9005**, with the message naming the specification revision:

```text
warning MCP9005: 'McpServer.SampleAsync(CreateMessageRequestParams, CancellationToken)'
is obsolete: 'The Sampling feature is deprecated as of specification version 2026-07-28
and may be removed in a future version. See SEP-2577 for more information.'
```

The same diagnostic fires for `McpServer.RequestRootsAsync`, `McpServer.LoggingLevel`, `McpClient.SetLoggingLevelAsync`, `WithSetLoggingLevelHandler`, `AsSamplingChatClient`, `AsClientLoggerProvider`, and the protocol types behind them — `CreateMessageRequestParams`, `SamplingMessage`, `ModelPreferences`, `Root`, `RootsCapability`, `LoggingLevel`, `SetLevelRequestParams`.

Nothing here stops compiling. But `AsSamplingChatClient` is how a lot of servers borrow the client's model, and treating it as a warning you will fix later is how it becomes a warning you fix under pressure. If your build treats warnings as errors, decide deliberately:

```xml
<PropertyGroup>
  <!-- Deliberate, time-boxed: sampling removal is not scheduled yet. -->
  <NoWarn>$(NoWarn);MCP9005</NoWarn>
</PropertyGroup>
```

Put that in the project that actually uses the API, not in `Directory.Build.props`. The whole value of a per-feature diagnostic id is lost the moment you silence it solution-wide — which is exactly the mistake that made the tasks removal a surprise.

## Stateful HTTP Is Now the Escape Hatch

The quietest change is the one to check first. `HttpServerTransportOptions.Stateless` defaults to **true** in 2.0.0. Every knob for the stateful Streamable HTTP path is obsolete under diagnostic **MCP9006**:

```text
warning MCP9006: 'HttpServerTransportOptions.IdleTimeout' is obsolete: 'Stateful
Streamable HTTP mode is a back-compat-only escape hatch for legacy clients. Set
HttpServerTransportOptions.Stateless = true (the default as of the 2026-07-28
protocol revision) for new code. See SEP-2567.'
```

That covers `IdleTimeout`, `MaxIdleSessionCount`, `PerSessionExecutionContext`, `SessionMigrationHandler`, `EventStreamStore`, `EnableLegacySse` and `WithDistributedCacheEventStreamStore`, plus the `ISseEventStreamStore` / `ISseEventStreamReader` / `ISseEventStreamWriter` interfaces.

If you tuned session lifetime for a multi-instance deployment, this is your migration. Stateless mode removes the need for the distributed event-stream store entirely — which is a simplification, not a loss — but any tool that stashed per-session state in an `AsyncLocal` behind `PerSessionExecutionContext` needs rethinking. Keeping the old behaviour is an explicit opt-in now:

```csharp
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Only if you must serve clients on an older protocol revision.
        options.Stateless = false;
    });
```

Treat that as a dated decision with a removal plan attached, not a permanent setting.

## What You Get in Return

The 2.0 additions track the 2026-07-28 protocol revision, and several are immediately useful.

**Per-request headers bind straight into tools.** The new `[McpHeader]` attribute pulls a header value into a tool parameter, which makes multi-tenant routing far less awkward than reaching for `IHttpContextAccessor`:

```csharp
[McpServerTool(Name = "get_shipment_status", ReadOnly = true)]
[Description("Returns the current status of a shipment.")]
public async Task<string> GetShipmentStatusAsync(
    [Description("Carrier tracking number.")] string trackingNumber,
    [McpHeader("x-tenant-id")] string tenantId,
    CancellationToken cancellationToken)
    => await tracker.GetStatusAsync(tenantId, trackingNumber, cancellationToken);
```

**OAuth callbacks got a real shape.** `ClientOAuthOptions.AuthorizationRedirectDelegate` is obsolete, replaced by `AuthorizationCallbackHandler`, which receives an `AuthorizationCallbackContext` (`AuthorizationUri`, `RedirectUri`) and returns an `AuthorizationResult` carrying `Code`, `State` and `Iss`. There is also a `ScopeSelector` hook for negotiating scopes per server.

**Errors are typed.** `UnsupportedProtocolVersionException` and `MissingRequiredClientCapabilityException` replace parsing failure out of a generic JSON-RPC error — worth catching explicitly at your transport boundary, because a version mismatch and a genuine bug now look different.

**Result caching and discovery.** `ICacheableResult` with `CacheScope` (`Public` / `Private`) and `TimeToLive` lets a server tell clients what may be cached, and `DiscoverRequestParams` / `DiscoverResult` add a discovery round trip. On the client, `AddKnownTools`, `RemoveKnownTools` and `ClearKnownTools` give you control over the tool list without a full re-handshake.

## Is It Worth Upgrading?

For a stdio server exposing a handful of tools: yes, and it will take an afternoon. Pin the version, run the build, fix nothing, ship it.

For an HTTP server with tuned sessions, sampling, or anything experimental: yes, but plan it as a piece of work rather than a dependency bump. The upgrade itself is mechanical; the design decisions it forces — stateless sessions, life after sampling — are not.

The one case for waiting is if you depend on a downstream package that has not moved to 2.0. Because 2.0.0 pins `ModelContextProtocol.Core` to an exact `[2.0.0]`, you cannot straddle the boundary while a dependency catches up. Check your transitive graph before you start:

```bash
dotnet list package --include-transitive | Select-String ModelContextProtocol
```

What you should not do is stay on an unpinned v1 reference. That is the worst of both worlds: you are on 2.0 already, you just have not noticed.

## Conclusion

MCP C# SDK 2.0 is a well-behaved major release. It deletes only what it had already marked experimental, deprecates the rest behind named diagnostics that tell you the specification revision and the proposal number, and leaves the everyday authoring surface — `AddMcpServer`, `WithTools<T>`, `[McpServerTool]` — untouched.

- **Pin the version.** The exact `[2.0.0]` pin on `ModelContextProtocol.Core` makes mixed graphs unresolvable.
- **Search for the tasks API before upgrading.** `IMcpTaskStore` and friends are deleted, not deprecated.
- **Suppress MCP9005 and MCP9006 narrowly, if at all.** Blanket `NoWarn` in shared props is what turns a deprecation into an outage.
- **Check `Stateless`.** It defaults to `true` now; stateful Streamable HTTP is a back-compat escape hatch with a shelf life.
- **Read the diagnostics.** Each one names its SEP, which is faster than any changelog.

If you followed our earlier post on [creating MCP servers in .NET](/blog/creating-mcp-servers-dotnet), the server code in it still compiles against 2.0 — but the unpinned `dotnet add package ModelContextProtocol` line in it no longer gives you what it gave you in June. Pin it, and work through the two diagnostics above.

