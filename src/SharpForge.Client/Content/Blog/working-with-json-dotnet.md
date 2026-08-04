---
title: "Working with JSON in .NET"
category: "Best Practices"
date: "December 10, 2025"
readTime: "8 min read"
excerpt: "Master System.Text.Json and learn when to use Newtonsoft.Json. Covers serialization, deserialization, and custom converters."
tags: ["JSON", "System.Text.Json", "Serialization", ".NET"]
sidebar:
  - href: "#system.text.json-basics"
    text: "System.Text.Json Basics"
  - href: "#common-options"
    text: "Common Options"
  - href: "#custom-converters"
    text: "Custom Converters"
  - href: "#polymorphic-serialization"
    text: "Polymorphic Serialization"
  - href: "#jsonnode-for-dynamic-json"
    text: "JsonNode"
  - href: "#when-to-use-newtonsoft.json"
    text: "When to Use Newtonsoft"
---

JSON is the universal data format for web APIs. .NET provides two main libraries for JSON handling: the built-in System.Text.Json and the popular Newtonsoft.Json.

## System.Text.Json Basics

```csharp
using System.Text.Json;

// Serialize
var user = new User { Name = "John", Age = 30 };
string json = JsonSerializer.Serialize(user);
// {"Name":"John","Age":30}

// Deserialize
var user2 = JsonSerializer.Deserialize<User>(json);

// Pretty print
var options = new JsonSerializerOptions { WriteIndented = true };
string prettyJson = JsonSerializer.Serialize(user, options);
```

## Common Options

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = { new JsonStringEnumConverter() }
};

// Reuse options for performance
builder.Services.AddSingleton(options);

// In ASP.NET Core
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = 
            JsonNamingPolicy.CamelCase;
    });
```

## Attributes for Control

```csharp
public class Product
{
    [JsonPropertyName("product_id")]
    public int Id { get; set; }

    [JsonPropertyName("product_name")]
    public string Name { get; set; } = "";

    [JsonIgnore]
    public string InternalCode { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonInclude]
    private string _privateField = "included";

    [JsonPropertyOrder(1)]
    public decimal Price { get; set; }
}
```

## Custom Converters

```csharp
public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options)
    {
        return DateOnly.ParseExact(reader.GetString()!, Format);
    }

    public override void Write(
        Utf8JsonWriter writer, 
        DateOnly value, 
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

// Register converter
var options = new JsonSerializerOptions
{
    Converters = { new DateOnlyConverter() }
};
```

## Polymorphic Serialization

```csharp
[JsonDerivedType(typeof(Cat), "cat")]
[JsonDerivedType(typeof(Dog), "dog")]
public abstract class Animal
{
    public string Name { get; set; } = "";
}

public class Cat : Animal
{
    public bool LikesCatnip { get; set; }
}

public class Dog : Animal
{
    public string FavoriteToy { get; set; } = "";
}

// Serializes with $type discriminator
var animals = new Animal[] { new Cat(), new Dog() };
var json = JsonSerializer.Serialize(animals);
// [{"$type":"cat","Name":"","LikesCatnip":false},...]
```

## Working with JSON Documents

```csharp
// Parse without deserializing to a type
using var doc = JsonDocument.Parse(json);
var root = doc.RootElement;

string name = root.GetProperty("name").GetString()!;
int age = root.GetProperty("age").GetInt32();

// Enumerate arrays
foreach (var item in root.GetProperty("items").EnumerateArray())
{
    Console.WriteLine(item.GetProperty("id").GetInt32());
}

// Check if property exists
if (root.TryGetProperty("optional", out var optional))
{
    // Use optional
}
```

## JsonNode for Dynamic JSON

```csharp
// Mutable JSON manipulation
var node = JsonNode.Parse(json);

// Read values
string? name = node?["name"]?.GetValue<string>();

// Modify values
node!["name"] = "New Name";
node["newProperty"] = 42;

// Create from scratch
var obj = new JsonObject
{
    ["name"] = "John",
    ["age"] = 30,
    ["tags"] = new JsonArray("developer", "blogger")
};

string result = obj.ToJsonString();
```

## Source Generators (AOT)

```csharp
// For Native AOT and better performance
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<Product>))]
public partial class AppJsonContext : JsonSerializerContext { }

// Usage
var json = JsonSerializer.Serialize(user, AppJsonContext.Default.User);
var user = JsonSerializer.Deserialize(json, AppJsonContext.Default.User);
```

## When to Use Newtonsoft.Json

Consider Newtonsoft.Json when you need:

- JSON Path queries
- LINQ to JSON with JObject/JArray
- More lenient parsing
- Compatibility with older libraries

```csharp
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// LINQ to JSON
var jObject = JObject.Parse(json);
var names = jObject["users"]!
    .Select(u => u["name"]!.Value<string>())
    .ToList();

// JSON Path
var results = jObject.SelectTokens("$.users[?(@.age > 25)]");
```

## Performance Tips

- Reuse `JsonSerializerOptions` instances
- Use source generators for AOT scenarios
- Use `Utf8JsonReader/Writer` for streaming
- Avoid parsing large documents fully into memory

## Conclusion

System.Text.Json is the recommended choice for most .NET applications. It's fast, built-in, and supports AOT compilation. Use Newtonsoft.Json when you need its advanced features.
