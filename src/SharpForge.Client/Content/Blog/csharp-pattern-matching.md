---
title: "C# Pattern Matching"
category: "C#"
date: "November 5, 2025"
readTime: "8 min read"
excerpt: "Master pattern matching in C# with practical examples. Covers switch expressions, property patterns, and positional patterns."
tags: ["C#", "Pattern Matching", "Switch Expressions"]
sidebar:
  - href: "#type-patterns"
    text: "Type Patterns"
  - href: "#property-patterns"
    text: "Property Patterns"
  - href: "#relational-patterns"
    text: "Relational Patterns"
  - href: "#positional-patterns"
    text: "Positional Patterns"
  - href: "#list-patterns-c-11"
    text: "List Patterns"
  - href: "#when-guards"
    text: "When Guards"
---

Pattern matching is one of C#'s most powerful features, enabling expressive and concise code for complex conditional logic.

## Type Patterns

```csharp
object obj = "Hello";

// is expression
if (obj is string s)
{
    Console.WriteLine(s.ToUpper());
}

// switch expression
string result = obj switch
{
    string str => $"String: {str}",
    int num => $"Integer: {num}",
    null => "Null value",
    _ => "Unknown type"
};
```

## Property Patterns

```csharp
public record Person(string Name, int Age, Address Address);
public record Address(string City, string Country);

string GetGreeting(Person person) => person switch
{
    { Age: < 18 } => "Hey there!",
    { Age: >= 65 } => "Good day!",
    { Name: "Admin" } => "Welcome back, Admin",
    { Address.Country: "USA" } => "Hello from the US!",
    _ => "Hello!"
};

// Nested property patterns
string GetLocation(Person person) => person switch
{
    { Address: { City: "New York", Country: "USA" } } => "NYC",
    { Address: { Country: "UK" } } => "United Kingdom",
    { Address.City: var city } => city,
    _ => "Unknown"
};
```

## Relational Patterns

```csharp
string GetTemperatureDescription(int temp) => temp switch
{
    < 0 => "Freezing",
    >= 0 and < 10 => "Cold",
    >= 10 and < 20 => "Mild",
    >= 20 and < 30 => "Warm",
    >= 30 => "Hot"
};

// With variables
decimal GetDiscount(int quantity) => quantity switch
{
    < 10 => 0m,
    >= 10 and < 50 => 0.05m,
    >= 50 and < 100 => 0.10m,
    >= 100 => 0.15m
};
```

## Logical Patterns

```csharp
// and, or, not
bool IsValidAge(int age) => age is >= 0 and <= 120;

bool IsWeekend(DayOfWeek day) => day is DayOfWeek.Saturday or DayOfWeek.Sunday;

bool IsNotNull(object? obj) => obj is not null;

// Complex combinations
string Classify(int number) => number switch
{
    < 0 => "Negative",
    0 => "Zero",
    > 0 and < 10 => "Single digit",
    >= 10 and < 100 => "Double digit",
    _ => "Large number"
};
```

## Positional Patterns

```csharp
public record Point(int X, int Y);

string GetQuadrant(Point point) => point switch
{
    (0, 0) => "Origin",
    ( > 0, > 0) => "Quadrant 1",
    ( < 0, > 0) => "Quadrant 2",
    ( < 0, < 0) => "Quadrant 3",
    ( > 0, < 0) => "Quadrant 4",
    (_, 0) => "On X-axis",
    (0, _) => "On Y-axis"
};

// Deconstruction with any type
public class Rectangle
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public void Deconstruct(out int width, out int height)
    {
        width = Width;
        height = Height;
    }
}

string DescribeShape(Rectangle rect) => rect switch
{
    (0, _) or (_, 0) => "Invalid",
    (var w, var h) when w == h => "Square",
    (var w, var h) when w > h => "Landscape",
    _ => "Portrait"
};
```

## List Patterns (C# 11+)

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };

string DescribeArray(int[] arr) => arr switch
{
    [] => "Empty",
    [var single] => $"Single element: {single}",
    [var first, var second] => $"Two elements: {first}, {second}",
    [var first, .., var last] => $"From {first} to {last}",
    [1, 2, 3, ..] => "Starts with 1, 2, 3",
    [.., 4, 5] => "Ends with 4, 5"
};

// Slice pattern
bool StartsWithZero(int[] arr) => arr is [0, ..];
bool EndsWithZero(int[] arr) => arr is [.., 0];
```

## Switch Statement Patterns

```csharp
void ProcessShape(Shape shape)
{
    switch (shape)
    {
        case Circle { Radius: 0 }:
            Console.WriteLine("Invalid circle");
            break;
            
        case Circle { Radius: var r } when r > 100:
            Console.WriteLine($"Large circle: {r}");
            break;
            
        case Circle c:
            Console.WriteLine($"Circle: {c.Radius}");
            break;
            
        case Rectangle { Width: var w, Height: var h } when w == h:
            Console.WriteLine($"Square: {w}");
            break;
            
        case Rectangle r:
            Console.WriteLine($"Rectangle: {r.Width}x{r.Height}");
            break;
            
        case null:
            throw new ArgumentNullException(nameof(shape));
            
        default:
            Console.WriteLine("Unknown shape");
            break;
    }
}
```

## When Guards

```csharp
string GetShippingCost(Order order) => order switch
{
    { Total: >= 100 } => "Free",
    { IsExpress: true } when order.Total < 50 => "$10.00",
    { IsExpress: true } => "$5.00",
    { Weight: <= 1 } => "$3.00",
    { Weight: <= 5 } => "$7.00",
    _ => "$12.00"
};
```

## Practical Examples

### HTTP Status Handling

```csharp
string HandleResponse(HttpResponseMessage response) => response.StatusCode switch
{
    HttpStatusCode.OK => "Success",
    HttpStatusCode.Created => "Resource created",
    HttpStatusCode.NoContent => "No content",
    >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError 
        => $"Client error: {response.StatusCode}",
    >= HttpStatusCode.InternalServerError 
        => $"Server error: {response.StatusCode}",
    _ => "Unknown status"
};
```

### Command Processing

```csharp
async Task ProcessCommand(Command cmd) => _ = cmd switch
{
    CreateUserCommand c => await CreateUserAsync(c),
    UpdateUserCommand c => await UpdateUserAsync(c),
    DeleteUserCommand { Id: var id } => await DeleteUserAsync(id),
    _ => throw new NotSupportedException()
};
```

## Best Practices

- Use switch expressions for simple mappings
- Prefer patterns over multiple if-else chains
- Always include a default case (discard pattern)
- Use when guards for complex conditions
- Combine patterns for expressive code
