---
title: "Memory Management in .NET: A Deep Dive"
category: "Performance"
date: "January 2, 2026"
readTime: "12 min read"
excerpt: "Understanding garbage collection, memory allocation, and optimization techniques for high-performance .NET applications."
tags: ["Memory Management", "Garbage Collection", "Performance", ".NET"]
sidebar:
  - href: "#the.net-memory-model"
    text: "Memory Model"
  - href: "#garbage-collection-generations"
    text: "GC Generations"
  - href: "#large-object-heap-loh"
    text: "Large Object Heap"
  - href: "#memory-optimization-techniques"
    text: "Optimization Techniques"
  - href: "#detecting-memory-issues"
    text: "Detecting Issues"
  - href: "#idisposable-pattern"
    text: "IDisposable Pattern"
---

Memory management is crucial for building high-performance .NET applications. Understanding how the garbage collector works and how to optimize memory usage can significantly improve your application's performance.

## The .NET Memory Model

.NET uses a managed heap for dynamic memory allocation. The runtime automatically handles memory allocation and deallocation through garbage collection.

### Stack vs Heap

- **Stack** - Stores value types and method call information. Fast allocation, automatic cleanup.
- **Heap** - Stores reference types. Managed by the garbage collector.

```csharp
// Value type - stored on stack
int number = 42;
DateTime date = DateTime.Now;

// Reference type - stored on heap
string name = "SharpForge";
var list = new List<int>();
```

## Garbage Collection Generations

The .NET GC uses a generational approach with three generations:

- **Generation 0** - Short-lived objects. Collected most frequently.
- **Generation 1** - Buffer between short and long-lived objects.
- **Generation 2** - Long-lived objects. Collected least frequently.

```csharp
// Check current generation of an object
var obj = new object();
int generation = GC.GetGeneration(obj);
Console.WriteLine($"Generation: {generation}"); // 0

// Force garbage collection (avoid in production)
GC.Collect();
GC.WaitForPendingFinalizers();
```

## Large Object Heap (LOH)

Objects larger than 85,000 bytes are allocated on the Large Object Heap. The LOH is collected with Generation 2 but is not compacted by default.

```csharp
// This goes to LOH (array of 85,000+ bytes)
var largeArray = new byte[100_000];

// Enable LOH compaction if needed
GCSettings.LargeObjectHeapCompactionMode = 
    GCLargeObjectHeapCompactionMode.CompactOnce;
```

## Memory Optimization Techniques

### 1. Use Structs for Small Data

```csharp
// ❌ Class - heap allocation
public class PointClass
{
    public int X { get; set; }
    public int Y { get; set; }
}

// ✅ Struct - stack allocation
public struct PointStruct
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

### 2. Object Pooling

```csharp
// Use ArrayPool for temporary arrays
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(1024);
try
{
    // Use the buffer
    ProcessData(buffer);
}
finally
{
    pool.Return(buffer);
}

// Use ObjectPool for custom objects
var objectPool = new DefaultObjectPool<StringBuilder>(
    new StringBuilderPooledObjectPolicy());
var sb = objectPool.Get();
try
{
    sb.Append("Hello");
    return sb.ToString();
}
finally
{
    objectPool.Return(sb);
}
```

### 3. Span&lt;T&gt; and Memory&lt;T&gt;

```csharp
// Avoid allocations with Span
public void ProcessData(ReadOnlySpan<byte> data)
{
    // No heap allocation - works on stack
    var slice = data.Slice(0, 10);
    foreach (var b in slice)
    {
        // Process byte
    }
}

// Substring without allocation
ReadOnlySpan<char> span = "Hello, World!".AsSpan();
ReadOnlySpan<char> hello = span.Slice(0, 5); // No allocation!
```

### 4. String Interning

```csharp
// Strings are immutable - each operation creates new string
string s1 = "Hello";
string s2 = s1 + " World"; // New allocation

// Use StringBuilder for multiple concatenations
var sb = new StringBuilder();
for (int i = 0; i < 100; i++)
{
    sb.Append(i);
    sb.Append(", ");
}
string result = sb.ToString();

// String interning for frequently used strings
string interned = string.Intern(someString);
```

## Detecting Memory Issues

### Using dotnet-counters

```bash
# Install the tool
dotnet tool install -g dotnet-counters

# Monitor GC metrics
dotnet-counters monitor -p <PID> --counters System.Runtime
```

### Using dotnet-dump

```bash
# Capture a memory dump
dotnet-dump collect -p <PID>

# Analyze the dump
dotnet-dump analyze <dump-file>

# Show heap statistics
> dumpheap -stat
```

## IDisposable Pattern

```csharp
public class ResourceHolder : IDisposable
{
    private bool _disposed;
    private IntPtr _unmanagedResource;
    private Stream _managedResource;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            _managedResource?.Dispose();
        }

        // Free unmanaged resources
        if (_unmanagedResource != IntPtr.Zero)
        {
            // Free unmanaged memory
            _unmanagedResource = IntPtr.Zero;
        }

        _disposed = true;
    }

    ~ResourceHolder() => Dispose(false);
}
```

## Best Practices

- Use `using` statements for IDisposable objects
- Avoid finalizers unless working with unmanaged resources
- Use object pooling for frequently allocated objects
- Prefer `Span<T>` over arrays for temporary data
- Use `StringBuilder` for string concatenation in loops
- Profile your application to identify memory hotspots

## Conclusion

Understanding .NET memory management helps you write more efficient applications. Use the tools and techniques described here to identify and fix memory issues, and always profile before optimizing.
