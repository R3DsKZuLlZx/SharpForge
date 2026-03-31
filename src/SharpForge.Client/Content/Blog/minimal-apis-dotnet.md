---
title: "Minimal APIs in .NET"
category: "ASP.NET Core"
date: "October 20, 2025"
readTime: "8 min read"
excerpt: "Build lightweight APIs with minimal code using .NET Minimal APIs. Perfect for microservices and simple endpoints."
tags: ["Minimal APIs", "ASP.NET Core", "REST", ".NET"]
sidebar:
  - href: "#hello-world"
    text: "Hello World"
  - href: "#route-handlers"
    text: "Route Handlers"
  - href: "#parameter-binding"
    text: "Parameter Binding"
  - href: "#route-groups"
    text: "Route Groups"
  - href: "#endpoint-filters"
    text: "Endpoint Filters"
  - href: "#openapi--swagger"
    text: "OpenAPI"
---

Minimal APIs provide a streamlined way to build HTTP APIs with minimal ceremony. They're perfect for microservices, small projects, or when you want less boilerplate.

## Hello World

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello, World!");

app.Run();
```

## Route Handlers

```csharp
// GET with route parameter
app.MapGet("/users/{id}", (int id) => $"User {id}");

// GET with query string
app.MapGet("/search", (string? q, int page = 1) => $"Search: {q}, Page: {page}");

// POST with body
app.MapPost("/users", (User user) => Results.Created($"/users/{user.Id}", user));

// PUT
app.MapPut("/users/{id}", (int id, User user) => Results.Ok(user));

// DELETE
app.MapDelete("/users/{id}", (int id) => Results.NoContent());

// Multiple HTTP methods
app.MapMethods("/resource", new[] { "GET", "POST" }, () => "GET or POST");
```

## Parameter Binding

```csharp
// Route, query, header, body - all automatic
app.MapGet("/products/{id}", (
    int id,                           // From route
    [FromQuery] string? filter,       // From query
    [FromHeader] string authorization,// From header
    HttpContext context               // Special types injected
) => { });

// Body binding
app.MapPost("/products", ([FromBody] Product product) => { });

// Services from DI
app.MapGet("/data", (IDataService service) => service.GetData());

// AsParameters for grouping
app.MapGet("/items", ([AsParameters] ItemQuery query) => { });

public record ItemQuery(
    string? Search,
    int Page = 1,
    int PageSize = 10
);
```

## Returning Results

```csharp
// Typed results
app.MapGet("/users/{id}", (int id, IUserService service) =>
{
    var user = service.GetById(id);
    return user is null 
        ? Results.NotFound() 
        : Results.Ok(user);
});

// TypedResults for OpenAPI
app.MapGet("/products/{id}", Results<Ok<Product>, NotFound> (int id) =>
{
    var product = GetProduct(id);
    return product is not null 
        ? TypedResults.Ok(product) 
        : TypedResults.NotFound();
});

// Other result types
Results.Json(data);
Results.File(bytes, "application/pdf");
Results.Redirect("/other");
Results.Problem("Error details");
Results.ValidationProblem(errors);
```

## Route Groups

```csharp
var api = app.MapGroup("/api");
var v1 = api.MapGroup("/v1");

var users = v1.MapGroup("/users")
    .WithTags("Users")
    .RequireAuthorization();

users.MapGet("/", GetAllUsers);
users.MapGet("/{id}", GetUser);
users.MapPost("/", CreateUser);
users.MapPut("/{id}", UpdateUser);
users.MapDelete("/{id}", DeleteUser);

// Methods can be organized elsewhere
static IResult GetAllUsers(IUserService service) => 
    Results.Ok(service.GetAll());

static IResult GetUser(int id, IUserService service) =>
    service.GetById(id) is User user 
        ? Results.Ok(user) 
        : Results.NotFound();
```

## Validation

```csharp
// Using FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

app.MapPost("/users", async (
    User user, 
    IValidator<User> validator) =>
{
    var result = await validator.ValidateAsync(user);
    
    if (!result.IsValid)
    {
        return Results.ValidationProblem(result.ToDictionary());
    }

    // Create user...
    return Results.Created($"/users/{user.Id}", user);
});

// Or use endpoint filter
app.MapPost("/products", CreateProduct)
    .AddEndpointFilter<ValidationFilter<Product>>();
```

## Endpoint Filters

```csharp
// Inline filter
app.MapGet("/protected", () => "Secret data")
    .AddEndpointFilter(async (context, next) =>
    {
        var apiKey = context.HttpContext.Request.Headers["X-API-Key"];
        if (apiKey != "secret")
        {
            return Results.Unauthorized();
        }
        return await next(context);
    });

// Filter class
public class LoggingFilter : IEndpointFilter
{
    private readonly ILogger _logger;

    public LoggingFilter(ILogger<LoggingFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        _logger.LogInformation("Handling {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);

        var result = await next(context);

        _logger.LogInformation("Completed with {Type}", result?.GetType().Name);
        
        return result;
    }
}

app.MapGet("/logged", Handler).AddEndpointFilter<LoggingFilter>();
```

## OpenAPI / Swagger

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/users", GetUsers)
    .WithName("GetUsers")
    .WithTags("Users")
    .WithDescription("Gets all users")
    .Produces<List<User>>()
    .ProducesProblem(500)
    .WithOpenApi(operation =>
    {
        operation.Summary = "Retrieve all users";
        return operation;
    });
```

## Authentication & Authorization

```csharp
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Require auth
app.MapGet("/secure", () => "Authenticated!")
    .RequireAuthorization();

// Specific policy
app.MapGet("/admin", () => "Admin only!")
    .RequireAuthorization("AdminPolicy");

// Allow anonymous
app.MapGet("/public", () => "Anyone!")
    .AllowAnonymous();
```

## Organization Patterns

```csharp
// Extension method pattern
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
    }

    private static IResult GetAll(IUserService service) =>
        Results.Ok(service.GetAll());

    private static IResult GetById(int id, IUserService service) =>
        service.GetById(id) is User u ? Results.Ok(u) : Results.NotFound();

    private static IResult Create(User user, IUserService service) =>
    {
        service.Create(user);
        return Results.Created($"/users/{user.Id}", user);
    }
}

// Usage in Program.cs
app.MapUserEndpoints();
```

## Best Practices

- Use route groups for organization
- Extract handlers into methods or classes
- Use TypedResults for better OpenAPI docs
- Implement validation with filters
- Add proper error handling
- Document with OpenAPI attributes
