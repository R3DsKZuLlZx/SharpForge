---
title: "MediatR Replacements: Wolverine & Mediator.SourceGenerators"
category: "Architecture"
date: "February 11, 2026"
readTime: "8 min read"
excerpt: "If you like the request/handler pattern from MediatR but want to switch, two modern alternatives worth exploring are Wolverine and Mediator.SourceGenerators. This post compares the approaches and shows minimal examples to get started."
tags: ["MediatR", "Wolverine", "Source Generators", "CQRS"]
sidebar:
  - href: "#quick-comparison"
    text: "Quick comparison"
  - href: "#wolverine"
    text: "Wolverine example"
  - href: "#mediatorsourcegenerators"
    text: "Mediator.SourceGenerators example"
  - href: "#side-by-side-when-to-use-which"
    text: "Side-by-side"
  - href: "#migration-tips"
    text: "Migration tips"
---

MediatR popularized the simple request/handler and in-process mediator pattern. Two modern alternatives aim to address different concerns:

- **Wolverine** - a runtime-first library with a message bus, durable transports, and advanced middleware/outbox features. It's focused on building robust messaging systems and supports both in-process and distributed workflows.
- **Mediator.SourceGenerators** - a compile-time approach that uses source generation to remove reflection and boot-time scanning, giving lower startup cost and smaller runtime overhead while preserving the familiar mediator API.

## Quick comparison

High level trade-offs:

- **Wolverine** brings a full message bus with dispatch, durable queues, outbox support, and transport adapters (HTTP, Amazon SQS, etc.). It trades a larger surface and runtime features for operational power.
- **Mediator.SourceGenerators** keeps the simple mediator surface but moves work to compile time. You get near-zero reflection cost at startup, smaller allocations, and straightforward migration for MediatR-style handlers.

## Wolverine

Conceptually Wolverine encourages message types and handler classes. Handlers are discovered and hosted by the Wolverine runtime. A tiny example (simplified):

```csharp
// Message
public record CreateOrder(Guid OrderId, decimal Amount);

// Handler (instance method)
public class CreateOrderHandler
{
    public Task Handle(CreateOrder message)
    {
        // process the order, persist, publish events, etc.
        return Task.CompletedTask;
    }
}

// Registration
builder.Services.AddWolverine();

// Sending via injected bus:
await bus.InvokeAsync(new CreateOrder(orderId, 12.34m));
```

Note: Wolverine also provides a durable message bus, middleware pipeline, and built-in features for retries and an outbox which is useful when you need reliable delivery across services. Wolverine was not primarily designed for the mediator pattern, but still provides a good alternative to MediatR.

## Mediator.SourceGenerators

Mediator.SourceGenerators keeps the familiar request/response handler model but generates the mediator wiring at compile time. A minimal MediatR-style example that maps well to a source-generated mediator looks like this:

```csharp
// Request/Response
public record GetUser(int Id);
public record UserDto(int Id, string Name);

// Handler
public class GetUserHandler : IRequestHandler<GetUser, UserDto>
{
    public Task<UserDto> Handle(GetUser request, CancellationToken ct)
    {
        // load user from DB
        return Task.FromResult(new UserDto(request.Id, "Alice"));
    }
}

// Registration
builder.Services.AddMediator();

// Sending via injected mediator:
var user = await mediator.Send(new GetUser(42));
```

The important bit is that the heavy lifting of finding handlers and building dispatch code happens at compile-time rather than using reflection at startup. That results in faster startup and fewer allocations while keeping the developer ergonomics similar to MediatR.

## Side-by-side: when to use which

- Pick **Wolverine** when you need a first-class messaging runtime: durable delivery, outbox patterns, and seamless local + remote dispatch. It fits well for microservices and workflows where messages need guaranteed delivery or cross-process routing.
- Pick **Mediator.SourceGenerators** when you want MediatR-like semantics with minimal runtime overhead. Its ideal for in-process command/query dispatch in low-latency services where you control deployment and don't need durable transports.
- If you need both low overhead and durable features, consider hybrid approaches: use a source-generated mediator for hot-path in-process calls and a message bus for cross-process, durable flows.

## Migration tips

- Start by keeping your public request/handler types the same; swap the mediator implementation and fix registration. Mediator.SourceGenerators is often the least invasive swap from MediatR.
- When moving to Wolverine, evaluate your handlers for side effects, transactions, and idempotency. Wolverine's outbox and durable transports are powerful but require designing for eventual delivery semantics.
- Measure: benchmark startup time, allocations, and end-to-end latency. These metrics will validate whether the compile-time or runtime approach improves your scenario.

## Conclusion

There's no one-size-fits-all replacement for MediatR. Wolverine and Mediator.SourceGenerators solve different problems - one focuses on rich messaging/runtime features, the other on compile-time performance and low runtime overhead. Choose based on whether you need durable, distributed messaging or lean, high-performance in-process dispatch.
