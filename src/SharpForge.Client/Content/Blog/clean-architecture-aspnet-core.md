---
title: "Clean Architecture with ASP.NET Core"
category: "Architecture"
date: "January 8, 2026"
readTime: "15 min read"
excerpt: "A practical guide to implementing clean architecture in your ASP.NET Core applications for better maintainability and testability."
tags: ["Clean Architecture", "ASP.NET Core", "CQRS", "MediatR"]
sidebar:
  - href: "#what-is-clean-architecture"
    text: "What is Clean Architecture?"
  - href: "#project-structure"
    text: "Project Structure"
  - href: "#domain-layer"
    text: "Domain Layer"
  - href: "#application-layer"
    text: "Application Layer"
  - href: "#infrastructure-layer"
    text: "Infrastructure Layer"
  - href: "#presentation-layer"
    text: "Presentation Layer"
  - href: "#benefits-of-clean-architecture"
    text: "Benefits"
---

Clean Architecture, popularized by Robert C. Martin (Uncle Bob), is an architectural pattern that emphasizes separation of concerns and independence from frameworks, databases, and external agencies. In this guide, we'll implement Clean Architecture in an ASP.NET Core application.

## What is Clean Architecture?

Clean Architecture organizes code into concentric layers, with dependencies pointing inward. The innermost layers contain business logic, while outer layers handle infrastructure concerns.

### The Layers

- **Domain (Entities)** - Core business objects and logic
- **Application (Use Cases)** - Application-specific business rules
- **Infrastructure** - External concerns (database, APIs, file system)
- **Presentation** - UI and API controllers

## Project Structure

Let's create a solution with proper project separation:

```text
MyApp/
├── src/
│   ├── MyApp.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Exceptions/
│   ├── MyApp.Application/
│   │   ├── Common/
│   │   ├── Features/
│   │   └── Interfaces/
│   ├── MyApp.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Services/
│   │   └── Identity/
│   └── MyApp.WebApi/
│       ├── Controllers/
│       ├── Middleware/
│       └── Program.cs
└── tests/
    ├── MyApp.Domain.Tests/
    ├── MyApp.Application.Tests/
    └── MyApp.Integration.Tests/
```

## Domain Layer

The Domain layer contains enterprise-wide business rules. It has no dependencies on other layers.

```csharp
// Domain/Entities/Product.cs
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public ProductStatus Status { get; private set; }

    private Product() { } // For EF Core

    public static Product Create(string name, Money price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required");

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Status = ProductStatus.Draft
        };
    }

    public void Publish()
    {
        if (Status != ProductStatus.Draft)
            throw new DomainException("Only draft products can be published");
        
        Status = ProductStatus.Published;
    }
}

// Domain/ValueObjects/Money.cs
public record Money(decimal Amount, string Currency)
{
    public static Money USD(decimal amount) => new(amount, "USD");
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

## Application Layer

The Application layer contains use cases and orchestrates the flow of data. We'll use CQRS with MediatR:

```csharp
// Application/Features/Products/Commands/CreateProduct.cs
public record CreateProductCommand(string Name, decimal Price) 
    : IRequest<Guid>;

public class CreateProductHandler 
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        IProductRepository repository, 
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request, 
        CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.Name, 
            Money.USD(request.Price));

        await _repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}

// Application/Features/Products/Queries/GetProduct.cs
public record GetProductQuery(Guid Id) : IRequest<ProductDto?>;

public class GetProductHandler 
    : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(
        GetProductQuery request, 
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            request.Id, cancellationToken);
        
        return product?.ToDto();
    }
}
```

## Infrastructure Layer

The Infrastructure layer implements interfaces defined in the Application layer:

```csharp
// Infrastructure/Persistence/ProductRepository.cs
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Product product, CancellationToken cancellationToken)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }
}

// Infrastructure/Persistence/AppDbContext.cs
public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
```

## Presentation Layer

The API controllers are thin and delegate to MediatR:

```csharp
// WebApi/Controllers/ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        CreateProductCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
    {
        var product = await _mediator.Send(new GetProductQuery(id));
        return product is null ? NotFound() : Ok(product);
    }
}
```

## Benefits of Clean Architecture

- **Testability** - Business logic can be tested without UI, database, or external services
- **Flexibility** - Easy to swap implementations (e.g., change database)
- **Maintainability** - Clear separation makes code easier to understand and modify
- **Independence** - Core business logic doesn't depend on frameworks

## Conclusion

Clean Architecture provides a solid foundation for building maintainable, testable applications. While it requires more initial setup, the benefits become apparent as your application grows. Start with the core domain, build outward, and always keep dependencies pointing inward.
