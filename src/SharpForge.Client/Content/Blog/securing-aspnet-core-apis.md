---
title: "Securing ASP.NET Core APIs"
category: "ASP.NET Core"
date: "November 25, 2025"
readTime: "12 min read"
excerpt: "Learn practical techniques for protecting your ASP.NET Core APIs. This covers authentication, authorization, token validation, and common pitfalls."
tags: ["Security", "JWT", "Authentication", "ASP.NET Core"]
sidebar:
  - href: "#jwt-authentication"
    text: "JWT Authentication"
  - href: "#authorization"
    text: "Authorization"
  - href: "#refresh-tokens"
    text: "Refresh Tokens"
  - href: "#api-key-authentication"
    text: "API Key Auth"
  - href: "#cors-configuration"
    text: "CORS"
  - href: "#best-practices"
    text: "Best Practices"
---

Security is critical for any API. This guide covers implementing robust authentication and authorization in ASP.NET Core applications.

## JWT Authentication

JSON Web Tokens (JWT) are the most common authentication method for APIs.

### Configuration

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

### Generating Tokens

```csharp
public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

## Authorization

### Role-Based Authorization

```csharp
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard() => Ok("Admin only!");

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("reports")]
    public IActionResult GetReports() => Ok("Admin or Manager");
}
```

### Policy-Based Authorization

```csharp
// Configure policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("MinimumAge", policy =>
        policy.RequireClaim("age")
              .RequireAssertion(ctx =>
              {
                  var age = int.Parse(ctx.User.FindFirst("age")?.Value ?? "0");
                  return age >= 18;
              }));

    options.AddPolicy("CanEditProducts", policy =>
        policy.Requirements.Add(new ProductEditRequirement()));
});

// Use policy
[Authorize(Policy = "CanEditProducts")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateProduct(int id, ProductDto dto)
```

### Custom Authorization Handler

```csharp
public class ProductEditRequirement : IAuthorizationRequirement { }

public class ProductEditHandler 
    : AuthorizationHandler<ProductEditRequirement, Product>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProductEditRequirement requirement,
        Product product)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (product.OwnerId.ToString() == userId ||
            context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// Register handler
builder.Services.AddSingleton<IAuthorizationHandler, ProductEditHandler>();
```

## Refresh Tokens

```csharp
public class AuthService
{
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _tokenRepository.GetAsync(refreshToken);
        
        if (storedToken is null || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityException("Invalid refresh token");
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        
        // Revoke old refresh token
        await _tokenRepository.RevokeAsync(refreshToken);
        
        // Generate new tokens
        var newAccessToken = _tokenService.GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken();
        
        await _tokenRepository.SaveAsync(new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        return new TokenResponse(newAccessToken, newRefreshToken);
    }
}
```

## API Key Authentication

```csharp
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        if (!await apiKeyService.IsValidAsync(apiKey!))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
```

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://myapp.com", "https://admin.myapp.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

app.UseCors("AllowSpecificOrigins");
```

## Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append(
        "Content-Security-Policy", 
        "default-src 'self'");
    
    await next();
});
```

## Best Practices

- Always use HTTPS in production
- Store secrets securely (Azure Key Vault, etc.)
- Use short-lived access tokens with refresh tokens
- Implement rate limiting
- Log authentication failures
- Use strong password policies
- Validate and sanitize all input
