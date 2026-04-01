---
title: "Async/Await Best Practices in C# 13"
category: "Best Practices"
date: "January 15, 2026"
readTime: "8 min read"
excerpt: "Master asynchronous programming with these essential tips, patterns, and common pitfalls to avoid in modern C# development."
tags: ["C#", "async/await", "Best Practices", "Performance"]
sidebar:
  - href: "#async-all-the-way"
    text: "Async All the Way"
  - href: "#use-configureawait-appropriately"
    text: "ConfigureAwait"
  - href: "#avoid-async-void"
    text: "Avoid Async Void"
  - href: "#use-valuetask-for-hot-paths"
    text: "ValueTask"
  - href: "#cancellation-token-support"
    text: "Cancellation Tokens"
  - href: "#parallel-async-operations"
    text: "Parallel Operations"
  - href: "#use-iasyncenumerable-for-streaming"
    text: "IAsyncEnumerable"
  - href: "#exception-handling"
    text: "Exception Handling"
---

Asynchronous programming is essential for building responsive applications in .NET. However, it comes with its own set of challenges and gotchas. In this article, we'll explore best practices for using async/await effectively in C# 13.

## Async All the Way

One of the most important rules: once you go async, stay async. Don't mix synchronous and asynchronous code by calling `.Result` or `.Wait()` on tasks.

### ❌ Don't Do This

```csharp
public string GetData()
{
    // This can cause deadlocks!
    var result = GetDataAsync().Result;
    return result;
}
```

### ✅ Do This Instead

```csharp
public async Task<string> GetDataAsync()
{
    var result = await GetDataFromApiAsync();
    return result;
}
```

## Use ConfigureAwait Appropriately

In library code, use `ConfigureAwait(false)` to avoid capturing the synchronization context when you don't need it.

```csharp
// In library/service code
public async Task<Data> FetchDataAsync()
{
    var response = await httpClient.GetAsync(url)
        .ConfigureAwait(false);
    
    var content = await response.Content.ReadAsStringAsync()
        .ConfigureAwait(false);
    
    return JsonSerializer.Deserialize<Data>(content);
}
```

**Note:** In ASP.NET Core, there's no synchronization context, so `ConfigureAwait(false)` has no effect. However, it's still a good practice for library code that might be used in other contexts.

## Avoid Async Void

Never use `async void` except for event handlers. Async void methods can't be awaited, and exceptions thrown from them can't be caught.

### ❌ Don't Do This

```csharp
// Exceptions here will crash the application!
public async void ProcessDataAsync()
{
    await SomeOperationAsync();
}
```

### ✅ Do This Instead

```csharp
public async Task ProcessDataAsync()
{
    await SomeOperationAsync();
}

// Only use async void for event handlers
private async void Button_Click(object sender, EventArgs e)
{
    try
    {
        await ProcessDataAsync();
    }
    catch (Exception ex)
    {
        // Handle the exception
        logger.LogError(ex, "Error processing data");
    }
}
```

## Use ValueTask for Hot Paths

When a method frequently completes synchronously, use `ValueTask` instead of `Task` to avoid allocations.

```csharp
public ValueTask<Data> GetCachedDataAsync(string key)
{
    if (cache.TryGetValue(key, out var data))
    {
        // No allocation when returning cached data
        return ValueTask.FromResult(data);
    }
    
    return new ValueTask<Data>(FetchAndCacheAsync(key));
}

private async Task<Data> FetchAndCacheAsync(string key)
{
    var data = await FetchFromDatabaseAsync(key);
    cache.Set(key, data);
    return data;
}
```

## Cancellation Token Support

Always accept and pass through cancellation tokens in async methods.

```csharp
public async Task<List<User>> GetUsersAsync(
    CancellationToken cancellationToken = default)
{
    var response = await httpClient.GetAsync(
        "/api/users", 
        cancellationToken);
    
    response.EnsureSuccessStatusCode();
    
    return await response.Content
        .ReadFromJsonAsync<List<User>>(cancellationToken);
}
```

## Parallel Async Operations

When you have multiple independent async operations, run them in parallel using `Task.WhenAll`.

### ❌ Sequential (Slower)

```csharp
var users = await GetUsersAsync();
var products = await GetProductsAsync();
var orders = await GetOrdersAsync();
```

### ✅ Parallel (Faster)

```csharp
var usersTask = GetUsersAsync();
var productsTask = GetProductsAsync();
var ordersTask = GetOrdersAsync();

await Task.WhenAll(usersTask, productsTask, ordersTask);

var users = usersTask.Result;
var products = productsTask.Result;
var orders = ordersTask.Result;
```

## Use IAsyncEnumerable for Streaming

For operations that return multiple items over time, use `IAsyncEnumerable`:

```csharp
public async IAsyncEnumerable<LogEntry> StreamLogsAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    await foreach (var line in ReadLinesAsync(cancellationToken))
    {
        if (TryParseLogEntry(line, out var entry))
        {
            yield return entry;
        }
    }
}

// Consuming the stream
await foreach (var log in StreamLogsAsync(cancellationToken))
{
    ProcessLog(log);
}
```

## Exception Handling

Handle exceptions properly in async code. Remember that exceptions are stored in the Task and rethrown when awaited.

```csharp
public async Task ProcessWithRetryAsync(int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await ProcessAsync();
            return; // Success
        }
        catch (HttpRequestException) when (i < maxRetries - 1)
        {
            // Wait before retrying
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
        }
    }
}
```

## Avoid Capturing Variables in Closures

Be careful with closures in async lambdas - they can cause unexpected behavior:

```csharp
// ❌ Bug: All tasks use the same 'i' value
var tasks = new List<Task>();
for (int i = 0; i < 10; i++)
{
    tasks.Add(Task.Run(async () => 
    {
        await ProcessAsync(i); // Wrong!
    }));
}

// ✅ Fixed: Capture the value
for (int i = 0; i < 10; i++)
{
    int captured = i;
    tasks.Add(Task.Run(async () => 
    {
        await ProcessAsync(captured); // Correct!
    }));
}
```

## Summary

Following these best practices will help you write more efficient, reliable, and maintainable async code:

- Go async all the way - avoid blocking calls
- Use ConfigureAwait(false) in library code
- Avoid async void (except for event handlers)
- Consider ValueTask for frequently synchronous operations
- Always support cancellation tokens
- Run independent operations in parallel
- Use IAsyncEnumerable for streaming scenarios
- Handle exceptions appropriately
- Be mindful of closures in async code
