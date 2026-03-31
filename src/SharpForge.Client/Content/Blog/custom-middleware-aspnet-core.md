---
title: "Creating Custom Middleware"
category: "ASP.NET Core"
date: "November 15, 2025"
readTime: "8 min read"
excerpt: "Build custom middleware components for ASP.NET Core to handle cross-cutting concerns like logging, caching, and error handling."
tags: ["Middleware", "ASP.NET Core", "Pipeline"]
sidebar:
  - href: "#middleware-basics"
    text: "Middleware Basics"
  - href: "#convention-based-middleware"
    text: "Convention-Based"
  - href: "#exception-handling-middleware"
    text: "Exception Handling"
  - href: "#response-caching-middleware"
    text: "Response Caching"
  - href: "#factory-based-middleware"
    text: "Factory-Based"
  - href: "#middleware-order"
    text: "Middleware Order"
---

Middleware in ASP.NET Core forms the request pipeline. Each middleware component can process requests, modify responses, or short-circuit the pipeline.

## Middleware Basics

```csharp
// Inline middleware
app.Use(async (context, next) =>
{
    // Before the next middleware
    Console.WriteLine($"Request: {context.Request.Path}");
    
    await next(); // Call the next middleware
    
    // After the next middleware
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

// Terminal middleware (doesn't call next)
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello, World!");
});
```

## Convention-Based Middleware

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "{Method} {Path} completed in {ElapsedMs}ms with {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
    }
}

// Extension method for clean registration
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}

// Usage
app.UseRequestLogging();
```

## Exception Handling Middleware

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, 400);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, 500);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception ex, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = ex.Message,
            statusCode
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Response Caching Middleware

```csharp
public class SimpleCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public SimpleCacheMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "GET")
        {
            await _next(context);
            return;
        }

        var cacheKey = context.Request.Path.ToString();
        
        if (_cache.TryGetValue(cacheKey, out byte[]? cachedResponse))
        {
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(cachedResponse);
            return;
        }

        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        memStream.Position = 0;
        var responseBody = memStream.ToArray();
        
        if (context.Response.StatusCode == 200)
        {
            _cache.Set(cacheKey, responseBody, TimeSpan.FromMinutes(5));
        }

        await originalBody.WriteAsync(responseBody);
        context.Response.Body = originalBody;
    }
}
```

## Correlation ID Middleware

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
```

## Factory-Based Middleware

```csharp
public class FactoryMiddleware : IMiddleware
{
    private readonly IUserService _userService;

    // Scoped dependencies work here!
    public FactoryMiddleware(IUserService userService)
    {
        _userService = userService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var user = await _userService.GetCurrentUserAsync();
        context.Items["CurrentUser"] = user;
        await next(context);
    }
}

// Must register as a service
builder.Services.AddScoped<FactoryMiddleware>();

// Use with UseMiddleware
app.UseMiddleware<FactoryMiddleware>();
```

## Conditional Middleware

```csharp
// Only apply to specific paths
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<ApiAuthMiddleware>();
    });

// Map to specific path
app.Map("/health", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        await context.Response.WriteAsync("Healthy");
    });
});
```

## Middleware Order

```csharp
// Order matters!
app.UseExceptionHandler();    // 1. Catch exceptions
app.UseHsts();                // 2. Security headers
app.UseHttpsRedirection();    // 3. HTTPS redirect
app.UseStaticFiles();         // 4. Static files
app.UseRouting();             // 5. Route matching
app.UseCors();                // 6. CORS
app.UseAuthentication();      // 7. Who are you?
app.UseAuthorization();       // 8. Are you allowed?
app.UseEndpoints();           // 9. Execute endpoint
```

## Best Practices

- Keep middleware focused on a single responsibility
- Use extension methods for clean registration
- Consider middleware order carefully
- Use IMiddleware for scoped dependencies
- Avoid blocking calls in middleware
- Handle exceptions appropriately
