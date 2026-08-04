---
title: "Understanding Blazor Render Modes and Hosting Models"
category: "Blazor"
date: "February 3, 2026"
readTime: "8 min read"
excerpt: "Blazor provides several ways to render components and to host applications. This post explains the trade-offs between render modes (WASM, Server, and automatic/prerendering options) and the common hosting models (Standalone WebAssembly vs ASP.NET Core hosted Web App)."
tags: ["Blazor", "WebAssembly", "Hosting"]
sidebar:
  - href: "#render-modes"
    text: "Render modes"
  - href: "#hosting-models-standalone-wasm-vs-web-app"
    text: "Hosting models"
  - href: "#trade-offs-practical-guidance"
    text: "Trade-offs"
  - href: "#small-example-snippets"
    text: "Examples"
---

Blazor offers flexibility: you can run .NET in the browser (WebAssembly), run server-rendered interactive components (Server) or combine approaches that prerender HTML and then hydrate with client-side interactivity. Choosing the right render mode and hosting model depends on performance, deployment, SEO and server resource considerations.

## Render modes

### WebAssembly (WASM)

Blazor WebAssembly runs your app's .NET runtime in the browser using WebAssembly. The app's UI and event handling execute on the client. Advantages and characteristics:

- No server round-trips for UI event handling which is excellent offline or disconnected scenarios.
- Static hosting is possible (CDN / GitHub Pages / static site hosts).
- Initial load can be larger as the runtime, app DLLs and assets are downloaded to the client.

### Server (Blazor Server)

Blazor Server keeps the component rendering and state on the server. A lightweight SignalR connection synchronizes UI diffs and events between client and server.

- Fast initial load (HTML is rendered on the server) and small client payloads.
- Requires persistent connection (SignalR) and server resources for each connected user (server-side memory/state).
- Good for intranet apps or scenarios where server control is required.

### Auto / Prerendering (hybrid patterns)

There are hybrid approaches that combine server prerendering with client-side interactivity. Commonly you'll see patterns where the app renders static HTML on the server (good for SEO and perceived load), then the client "hydrates" - attaches interactivity - once the runtime is available.

Key options you might encounter:

- Server-side prerendering: the server emits initial HTML and the Blazor Server connection continues for live updates.
- WASM prerendering: generate initial HTML on the server, then boot the WASM runtime to take over client-side rendering.
- Auto / mixed strategies: pick prerendering when it helps (SEO, time-to-first-paint) and then hydrate with WASM or continue with Server.

## Hosting models - Standalone WASM vs Web App

### Standalone WebAssembly (Static)

Standalone (sometimes called "static") WASM apps are published as static files: an index.html, an app .dll bundle, and _framework assets. They can be hosted on static hosts like GitHub Pages, Netlify, Azure Static Web Apps or any CDN.

Pros:

- Cheap and simple hosting - no server required beyond static file serving.
- Scales easily via CDN.
- Works well if your app is primarily client-side and doesn't need server APIs beyond REST endpoints.

### ASP.NET Core hosted (Web App)

This model uses an ASP.NET Core backend that serves the WebAssembly app and can also host APIs, server-side prerendering, authentication, and more. The project template usually creates three projects: Client (WASM), Server (ASP.NET Core), and Shared.

Pros:

- Unified deployment for APIs and the client app.
- Supports prerendering, server-side routing fallbacks and custom middleware.
- Easier to integrate server-side features (authentication, server-side rendering, DI, etc.).

## Trade-offs & practical guidance

Use this quick checklist when choosing:

- If you need the lightest possible server footprint and easy hosting then **Standalone WASM** is ideal.
- If you require low TTFB (time to first byte) and SEO-critical content, consider **prerendering** and either Blazor Server or WASM with server prerendering.
- If your app needs frequent server coordination/state and you don't want to download the runtime to every client, lean toward **Blazor Server**.
- If you want the best of both worlds (SEO + client interactivity) use **WASM with prerender** - emit initial HTML from the server, then hydrate on the client.

## Small example snippets

WASM Program startup (client-side):

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddScoped<HttpClient>(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
await builder.Build().RunAsync();
```

Blazor Server Program startup (server-side):

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddServerSideBlazor();

var app = builder.Build();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
```

## Conclusion

Blazor's flexibility is a strength: you can choose client-side WASM for offline, low-server-cost deployments, server-side for thin clients and tight server control, or hybrid approaches to balance SEO and interactivity. The hosting model (Standalone vs Web App) determines your deployment surface and how easily you integrate server-side services.

If you're deciding for a new project, make a small spike: measure initial load, memory, server costs and developer experience. That data will guide a confident choice.
