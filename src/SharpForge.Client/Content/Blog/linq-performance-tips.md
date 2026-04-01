---
title: "LINQ Performance Tips"
category: "Performance"
date: "December 1, 2025"
readTime: "8 min read"
excerpt: "Optimize your LINQ queries with these practical tips. Learn about deferred execution, materialization, and common pitfalls."
tags: ["LINQ", "Performance", "C#", "Best Practices"]
sidebar:
  - href: "#understanding-deferred-execution"
    text: "Deferred Execution"
  - href: "#use-the-right-method"
    text: "Right Methods"
  - href: "#avoid-allocations"
    text: "Avoid Allocations"
  - href: "#database-query-optimization"
    text: "Database Queries"
  - href: "#parallel-linq-plinq"
    text: "PLINQ"
---

LINQ is powerful and expressive, but it can also be a source of performance issues if not used carefully. This guide covers essential tips for writing efficient LINQ queries.

## Understanding Deferred Execution

Most LINQ operations use deferred execution - they don't execute until you iterate over the results.

```csharp
var query = products
    .Where(p => p.Price > 100)
    .Select(p => p.Name);
// No execution yet!

// Execution happens here:
foreach (var name in query)
{
    Console.WriteLine(name);
}

// Or here:
var list = query.ToList(); // Materializes the query
```

### Multiple Enumeration Problem

```csharp
// ❌ BAD: Enumerates twice
var filtered = products.Where(p => p.IsActive);
Console.WriteLine($"Count: {filtered.Count()}"); // First enumeration
foreach (var p in filtered) { } // Second enumeration

// ✅ GOOD: Materialize once
var filtered = products.Where(p => p.IsActive).ToList();
Console.WriteLine($"Count: {filtered.Count}");
foreach (var p in filtered) { }
```

## Use the Right Method

### Any() vs Count()

```csharp
// ❌ SLOW: Counts all items
if (products.Count() > 0) { }

// ✅ FAST: Stops at first item
if (products.Any()) { }

// ❌ SLOW: Counts all matching items
if (products.Count(p => p.IsActive) > 0) { }

// ✅ FAST: Stops at first match
if (products.Any(p => p.IsActive)) { }
```

### First() vs Single()

```csharp
// First() - returns first match, stops immediately
var first = products.First(p => p.Id == 5);

// Single() - scans ALL items to ensure only one match
var single = products.Single(p => p.Id == 5);

// Use First when you know there's at least one
// Use Single when uniqueness must be enforced
```

### Find() for Lists

```csharp
// For List<T>, Find() is slightly faster than FirstOrDefault()
var list = new List<Product>();

// ✅ Slightly faster
var product = list.Find(p => p.Id == 5);

// Also fine
var product2 = list.FirstOrDefault(p => p.Id == 5);
```

## Avoid Allocations

### Use Span-based Methods

```csharp
// ❌ Allocates array
var chars = myString.ToCharArray();

// ✅ No allocation
ReadOnlySpan<char> span = myString.AsSpan();

// ❌ Allocates with LINQ
var hasDigit = myString.Any(char.IsDigit);

// ✅ No allocation with loop
bool hasDigit = false;
foreach (var c in myString.AsSpan())
{
    if (char.IsDigit(c)) { hasDigit = true; break; }
}
```

### Preallocate Collections

```csharp
// ❌ Multiple reallocations
var results = source.Select(Transform).ToList();

// ✅ Preallocate if count is known
var results = new List<Result>(source.Count);
foreach (var item in source)
{
    results.Add(Transform(item));
}
```

## Database Query Optimization

### Select Only What You Need

```csharp
// ❌ Loads entire entity
var products = await context.Products.ToListAsync();
var names = products.Select(p => p.Name);

// ✅ Projects in database
var names = await context.Products
    .Select(p => p.Name)
    .ToListAsync();
```

### Use AsNoTracking

```csharp
// ❌ Tracks all entities (memory overhead)
var products = await context.Products.ToListAsync();

// ✅ No tracking for read-only queries
var products = await context.Products
    .AsNoTracking()
    .ToListAsync();
```

### Avoid N+1 Queries

```csharp
// ❌ N+1 problem
var orders = await context.Orders.ToListAsync();
foreach (var order in orders)
{
    var items = order.Items; // Lazy load = N queries!
}

// ✅ Eager load
var orders = await context.Orders
    .Include(o => o.Items)
    .ToListAsync();
```

## OrderBy Placement

```csharp
// ❌ Sorts, then filters (sorts more items)
var result = products
    .OrderBy(p => p.Name)
    .Where(p => p.Price > 100)
    .Take(10);

// ✅ Filters first, then sorts (sorts fewer items)
var result = products
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .Take(10);
```

## Parallel LINQ (PLINQ)

```csharp
// For CPU-bound operations on large collections
var results = products
    .AsParallel()
    .Where(p => ExpensiveFilter(p))
    .Select(p => ExpensiveTransform(p))
    .ToList();

// Control degree of parallelism
var results = products
    .AsParallel()
    .WithDegreeOfParallelism(4)
    .Select(p => Process(p))
    .ToList();
```

## Benchmark Your Changes

```csharp
[MemoryDiagnoser]
public class LinqBenchmarks
{
    private List<int> numbers = Enumerable.Range(1, 10000).ToList();

    [Benchmark(Baseline = true)]
    public bool AnyWithPredicate() => numbers.Any(n => n == 5000);

    [Benchmark]
    public bool ContainsMethod() => numbers.Contains(5000);
}
```

## Summary

- Materialize queries to avoid multiple enumeration
- Use `Any()` instead of `Count() > 0`
- Filter before sorting and projecting
- Use `AsNoTracking()` for read-only EF queries
- Include related data to avoid N+1
- Always benchmark before and after optimization
