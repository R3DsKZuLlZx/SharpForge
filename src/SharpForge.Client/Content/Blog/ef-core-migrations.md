---
title: "Entity Framework Core Migrations"
category: "Entity Framework"
date: "November 20, 2025"
readTime: "10 min read"
excerpt: "Master database migrations in EF Core. Learn about migration strategies, seeding data, and handling production deployments."
tags: ["Entity Framework", "Migrations", "Database", ".NET"]
sidebar:
  - href: "#getting-started"
    text: "Getting Started"
  - href: "#custom-migrations"
    text: "Custom Migrations"
  - href: "#data-seeding"
    text: "Data Seeding"
  - href: "#production-strategies"
    text: "Production Strategies"
  - href: "#best-practices"
    text: "Best Practices"
---

Migrations are EF Core's way of keeping your database schema in sync with your model. This guide covers everything from basic migrations to production deployment strategies.

## Getting Started

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Add design package to your project
dotnet add package Microsoft.EntityFrameworkCore.Design
```

## Basic Commands

```bash
# Create a migration
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script

# List migrations
dotnet ef migrations list
```

## Migration Files

Each migration creates three files:

```
Migrations/
├── 20251120_InitialCreate.cs          // Up/Down methods
├── 20251120_InitialCreate.Designer.cs  // Snapshot metadata
└── AppDbContextModelSnapshot.cs        // Current model state
```

### Migration Code

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Products");
    }
}
```

## Custom Migrations

### Adding SQL

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create table first
    migrationBuilder.CreateTable(...);

    // Then add custom SQL
    migrationBuilder.Sql(@"
        CREATE INDEX IX_Products_Name 
        ON Products(Name) 
        INCLUDE (Price)
    ");

    // Or call a stored procedure
    migrationBuilder.Sql("EXEC sp_UpdateProductStats");
}
```

### Data Migrations

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add new column
    migrationBuilder.AddColumn<string>(
        name: "Slug",
        table: "Products",
        nullable: true);

    // Populate with data
    migrationBuilder.Sql(@"
        UPDATE Products 
        SET Slug = LOWER(REPLACE(Name, ' ', '-'))
    ");

    // Make it required
    migrationBuilder.AlterColumn<string>(
        name: "Slug",
        table: "Products",
        nullable: false,
        oldNullable: true);
}
```

## Data Seeding

### In OnModelCreating

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Category>().HasData(
        new Category { Id = 1, Name = "Electronics" },
        new Category { Id = 2, Name = "Clothing" },
        new Category { Id = 3, Name = "Books" }
    );

    // For owned types
    modelBuilder.Entity<Product>().OwnsOne(p => p.Price).HasData(
        new { ProductId = 1, Amount = 99.99m, Currency = "USD" }
    );
}
```

### Using Migrations

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "Categories",
        columns: new[] { "Id", "Name" },
        values: new object[,]
        {
            { 1, "Electronics" },
            { 2, "Clothing" },
            { 3, "Books" }
        });
}
```

## Production Strategies

### 1. SQL Scripts

```bash
# Generate idempotent script
dotnet ef migrations script --idempotent -o migrate.sql

# Generate script from specific migration
dotnet ef migrations script PreviousMigration CurrentMigration
```

### 2. Migration Bundles

```bash
# Create a self-contained executable
dotnet ef migrations bundle --self-contained

# Run the bundle
./efbundle --connection "Server=...;Database=..."
```

### 3. Runtime Migration

```csharp
// Apply migrations at startup (use with caution)
public static void Main(string[] args)
{
    var host = CreateHostBuilder(args).Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    host.Run();
}
```

## Handling Multiple Environments

```bash
# Use different connection strings
dotnet ef database update --connection "Server=prod;..."

# Or use environment variables
$env:ConnectionStrings__Default = "Server=prod;..."
dotnet ef database update
```

## Rolling Back

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Rollback all migrations
dotnet ef database update 0

# Then remove the migration files
dotnet ef migrations remove
```

## Best Practices

- Always review generated migration code
- Test migrations on a copy of production data
- Use idempotent scripts for production
- Keep migrations small and focused
- Never edit migrations that have been applied to production
- Include Down() methods for rollback capability
- Use transactions for data migrations
- Backup database before applying migrations

## Troubleshooting

```bash
# Rebuild snapshot if corrupted
dotnet ef migrations remove --force
dotnet ef migrations add Fresh

# Check pending migrations
dotnet ef migrations list

# Verbose output for debugging
dotnet ef database update --verbose
```
