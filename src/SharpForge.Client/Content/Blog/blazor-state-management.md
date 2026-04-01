---
title: "Blazor State Management"
category: "Blazor"
date: "November 10, 2025"
readTime: "10 min read"
excerpt: "Explore different approaches to state management in Blazor applications, from simple patterns to Flux-like architectures."
tags: ["Blazor", "State Management", "Fluxor"]
sidebar:
  - href: "#component-state"
    text: "Component State"
  - href: "#cascading-values"
    text: "Cascading Values"
  - href: "#state-container-service"
    text: "State Container"
  - href: "#fluxredux-pattern"
    text: "Flux/Redux"
  - href: "#browser-storage"
    text: "Browser Storage"
---

As Blazor applications grow, managing state becomes increasingly important. This guide covers various state management approaches from simple to complex.

## Component State

The simplest form - state lives in the component:

```razor
@code {
    private int count = 0;
    private List<Item> items = new();

    private void Increment() => count++;

    private void AddItem(Item item)
    {
        items.Add(item);
        // Component automatically re-renders
    }
}
```

## Cascading Values

Share state down the component tree:

```razor
<!-- Parent component -->
<CascadingValue Value="@currentUser">
    <CascadingValue Value="@theme">
        @ChildContent
    </CascadingValue>
</CascadingValue>

@code {
    private User currentUser = new();
    private Theme theme = Theme.Dark;
}

<!-- Child component -->
@code {
    [CascadingParameter]
    public User? CurrentUser { get; set; }

    [CascadingParameter]
    public Theme Theme { get; set; }
}
```

## State Container Service

A service that holds and notifies about state changes:

```csharp
public class AppState
{
    private int _count;
    public int Count => _count;

    public event Action? OnChange;

    public void IncrementCount()
    {
        _count++;
        NotifyStateChanged();
    }

    public void SetCount(int value)
    {
        _count = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Register as singleton or scoped
builder.Services.AddScoped<AppState>();
```

### Using the State Container

```razor
@inject AppState State
@implements IDisposable

<p>Count: @State.Count</p>
<button @onclick="State.IncrementCount">Increment</button>

@code {
    protected override void OnInitialized()
    {
        State.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        State.OnChange -= StateHasChanged;
    }
}
```

## Generic State Container

```csharp
public class StateContainer<T> where T : class, new()
{
    private T _state = new();
    public T State => _state;

    public event Action? OnChange;

    public void SetState(Action<T> updateAction)
    {
        updateAction(_state);
        NotifyStateChanged();
    }

    public void SetState(T newState)
    {
        _state = newState;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Usage
public class CartState
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Price * i.Quantity);
}

builder.Services.AddScoped<StateContainer<CartState>>();
```

## Flux/Redux Pattern

For complex applications, use a unidirectional data flow:

```csharp
// State
public record AppState(int Count, List<Todo> Todos);

// Actions
public abstract record AppAction;
public record IncrementAction : AppAction;
public record AddTodoAction(Todo Todo) : AppAction;
public record RemoveTodoAction(Guid Id) : AppAction;

// Reducer
public static class Reducers
{
    public static AppState Reduce(AppState state, AppAction action)
    {
        return action switch
        {
            IncrementAction => state with { Count = state.Count + 1 },
            AddTodoAction a => state with 
            { 
                Todos = state.Todos.Append(a.Todo).ToList() 
            },
            RemoveTodoAction a => state with 
            { 
                Todos = state.Todos.Where(t => t.Id != a.Id).ToList() 
            },
            _ => state
        };
    }
}

// Store
public class Store
{
    private AppState _state = new(0, new List<Todo>());
    public AppState State => _state;
    public event Action? OnChange;

    public void Dispatch(AppAction action)
    {
        _state = Reducers.Reduce(_state, action);
        OnChange?.Invoke();
    }
}
```

### Using Fluxor Library

```razor
// Install Fluxor
dotnet add package Fluxor.Blazor.Web

// State
[FeatureState]
public record CounterState(int Count)
{
    public CounterState() : this(0) { }
}

// Actions
public record IncrementCounterAction;

// Reducers
public static class CounterReducers
{
    [ReducerMethod]
    public static CounterState Reduce(CounterState state, IncrementCounterAction action)
        => state with { Count = state.Count + 1 };
}

// Component
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
@inject IState<CounterState> CounterState
@inject IDispatcher Dispatcher

<p>Count: @CounterState.Value.Count</p>
<button @onclick="Increment">Increment</button>

@code {
    private void Increment() => Dispatcher.Dispatch(new IncrementCounterAction());
}
```

## Browser Storage

```razor
// Persist state to localStorage
@inject IJSRuntime JS

@code {
    private async Task SaveStateAsync<T>(string key, T state)
    {
        var json = JsonSerializer.Serialize(state);
        await JS.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    private async Task<T?> LoadStateAsync<T>(string key)
    {
        var json = await JS.InvokeAsync<string?>("localStorage.getItem", key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }
}

// Or use a library like Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();
```

## Choosing the Right Approach

| Approach | Use When |
|----------|----------|
| Component State | State is local to one component |
| Cascading Values | Parent-child state sharing |
| State Container | Shared state across unrelated components |
| Flux/Redux | Complex state with predictable updates |

## Best Practices

- Start simple, add complexity as needed
- Always unsubscribe from events in Dispose
- Use immutable state where possible
- Consider using records for state objects
- Persist important state to browser storage
