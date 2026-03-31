---
title: "Dependency Injection Deep Dive"
category: "ASP.NET Core"
date: "December 15, 2025"
readTime: "12 min read"
excerpt: "Understanding the built-in dependency injection container in ASP.NET Core and advanced patterns for complex scenarios."
tags: ["Dependency Injection", "ASP.NET Core", "Design Patterns"]
sidebar:
  - href: "#service-lifetimes"
    text: "Service Lifetimes"
  - href: "#registration-patterns"
    text: "Registration Patterns"
  - href: "#advanced-patterns"
    text: "Advanced Patterns"
  - href: "#options-pattern"
    text: "Options Pattern"
  - href: "#avoiding-common-pitfalls"
    text: "Avoiding Pitfalls"
---

Dependency Injection (DI) is a fundamental pattern in ASP.NET Core. This deep dive explores the built-in container and advanced techniques for real-world applications.

## Service Lifetimes

Understanding lifetimes is crucial for correct DI usage:

```csharp
// Transient - new instance every time
builder.Services.AddTransient<IEmailService, EmailService>();

// Scoped - one instance per request
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Singleton - one instance for application lifetime
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

### Lifetime Validation

```csharp
// Enable scope validation in development
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
```

## Registration Patterns

### Multiple Implementations

```csharp
// Register multiple implementations
builder.Services.AddTransient<INotificationService, EmailNotification>();
builder.Services.AddTransient<INotificationService, SmsNotification>();
builder.Services.AddTransient<INotificationService, PushNotification>();

// Inject all implementations
public class NotificationManager
{
    private readonly IEnumerable<INotificationService> _services;

    public NotificationManager(
        IEnumerable<INotificationService> services)
    {
        _services = services;
    }

    public async Task NotifyAllAsync(string message)
    {
        foreach (var service in _services)
        {
            await service.SendAsync(message);
        }
    }
}
```

### Keyed Services (.NET 8+)

```csharp
// Register with keys
builder.Services.AddKeyedTransient<IPaymentProcessor, StripeProcessor>("stripe");
builder.Services.AddKeyedTransient<IPaymentProcessor, PayPalProcessor>("paypal");

// Inject specific implementation
public class CheckoutService
{
    public CheckoutService(
        [FromKeyedServices("stripe")] IPaymentProcessor processor)
    {
        // Uses StripeProcessor
    }
}
```

### Factory Pattern

```csharp
builder.Services.AddTransient<IOrderService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<OrderService>>();
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("Orders");
    
    return new OrderService(logger, connectionString);
});
```

## Advanced Patterns

### Decorator Pattern

```csharp
// Original service
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Decorate with caching
builder.Services.Decorate<IProductRepository, CachedProductRepository>();

public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedProductRepository(
        IProductRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _cache.GetOrCreateAsync(
            $"product:{id}",
            async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                return await _inner.GetByIdAsync(id);
            });
    }
}
```

### Open Generics

```csharp
// Register open generic
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Usage - automatically resolves closed generic
public class ProductService
{
    private readonly IRepository<Product> _repository;

    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }
}
```

### Conditional Registration

```csharp
// TryAdd - only adds if not already registered
builder.Services.TryAddScoped<IUserService, UserService>();

// Replace existing registration
builder.Services.Replace(
    ServiceDescriptor.Scoped<IUserService, NewUserService>());

// Remove a registration
builder.Services.RemoveAll<IUserService>();
```

## Options Pattern

```csharp
// Configuration class
public class EmailSettings
{
    public string SmtpServer { get; set; } = "";
    public int Port { get; set; }
    public string FromAddress { get; set; } = "";
}

// Register from configuration
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

// Inject options
public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }
}

// Use IOptionsSnapshot for scoped (reloads on change)
// Use IOptionsMonitor for singleton (notifies on change)
```

## Avoiding Common Pitfalls

### Captive Dependencies

```csharp
// ❌ BAD: Singleton captures Scoped service
public class SingletonService
{
    private readonly IScopedService _scoped; // Will cause issues!
}

// ✅ GOOD: Use IServiceScopeFactory
public class SingletonService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SingletonService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task DoWorkAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var scopedService = scope.ServiceProvider
            .GetRequiredService<IScopedService>();
        await scopedService.ProcessAsync();
    }
}
```

### Service Locator Anti-Pattern

```csharp
// ❌ BAD: Service Locator
public class BadService
{
    private readonly IServiceProvider _provider;

    public void DoWork()
    {
        var service = _provider.GetService<IMyService>(); // Anti-pattern!
    }
}

// ✅ GOOD: Constructor Injection
public class GoodService
{
    private readonly IMyService _service;

    public GoodService(IMyService service)
    {
        _service = service;
    }
}
```

## Testing with DI

```csharp
public class UserServiceTests
{
    [Fact]
    public async Task GetUser_ReturnsUser()
    {
        // Arrange - create test services
        var services = new ServiceCollection();
        services.AddScoped<IUserRepository, FakeUserRepository>();
        services.AddScoped<IUserService, UserService>();
        
        var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<IUserService>();

        // Act
        var result = await sut.GetUserAsync(1);

        // Assert
        Assert.NotNull(result);
    }
}
```

## Conclusion

ASP.NET Core's built-in DI container is powerful enough for most applications. Understanding lifetimes, registration patterns, and common pitfalls will help you build maintainable, testable applications.
