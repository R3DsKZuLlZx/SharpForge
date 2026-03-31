---
title: "Blazor Component Lifecycle"
category: "Blazor"
date: "December 5, 2025"
readTime: "10 min read"
excerpt: "Deep dive into Blazor component lifecycle methods and how to use them effectively in your applications."
tags: ["Blazor", "Components", "Lifecycle", ".NET"]
sidebar:
  - href: "#lifecycle-overview"
    text: "Overview"
  - href: "#setparametersasync"
    text: "SetParametersAsync"
  - href: "#oninitialized--oninitializedasync"
    text: "OnInitialized"
  - href: "#onparametersset--onparameterssetasync"
    text: "OnParametersSet"
  - href: "#onafterrender--onafterrenderasync"
    text: "OnAfterRender"
  - href: "#shouldrender"
    text: "ShouldRender"
  - href: "#statehaschanged"
    text: "StateHasChanged"
  - href: "#idisposable--iasyncdisposable"
    text: "IDisposable"
  - href: "#lifecycle-diagram"
    text: "Lifecycle Diagram"
---

Understanding the Blazor component lifecycle is essential for building efficient and responsive applications. This guide covers each lifecycle method and when to use them.

## Lifecycle Overview

Blazor components go through a series of lifecycle events from creation to disposal:

1. `SetParametersAsync`
2. `OnInitialized` / `OnInitializedAsync`
3. `OnParametersSet` / `OnParametersSetAsync`
4. `OnAfterRender` / `OnAfterRenderAsync`
5. `Dispose` / `DisposeAsync`

## SetParametersAsync

Called when parameters are set or updated. Rarely overridden directly.

```csharp
public override async Task SetParametersAsync(ParameterView parameters)
{
    // Access parameters before they're applied
    if (parameters.TryGetValue<string>("Title", out var title))
    {
        Console.WriteLine($"Title will be: {title}");
    }

    // Must call base to apply parameters
    await base.SetParametersAsync(parameters);
}
```

## OnInitialized / OnInitializedAsync

Called once when the component is first created. Perfect for initial data loading.

```razor
@code {
    private List<Product>? products;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            products = await ProductService.GetProductsAsync();
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

### Important Notes

- Called only once, not on parameter changes
- Parameters are available at this point
- For Blazor Server, called twice during prerendering (use `OnAfterRenderAsync` for one-time operations)

## OnParametersSet / OnParametersSetAsync

Called after parameters are set, both initially and on updates.

```razor
@code {
    [Parameter]
    public int ProductId { get; set; }

    private Product? product;
    private int previousProductId;

    protected override async Task OnParametersSetAsync()
    {
        // Only reload if ProductId changed
        if (ProductId != previousProductId)
        {
            previousProductId = ProductId;
            product = await ProductService.GetByIdAsync(ProductId);
        }
    }
}
```

## OnAfterRender / OnAfterRenderAsync

Called after the component has rendered. Use for DOM interactions and JavaScript interop.

```razor
@inject IJSRuntime JS

@code {
    private ElementReference inputElement;
    private bool hasRendered;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Focus the input on first render
            await inputElement.FocusAsync();
            
            // Initialize JS library
            await JS.InvokeVoidAsync("initializeChart", "chartContainer");
            
            hasRendered = true;
        }
    }
}
```

### The firstRender Parameter

- `true` only on the first render
- `false` on subsequent renders
- Use for one-time initialization

## ShouldRender

Controls whether the component should re-render. Useful for performance optimization.

```razor
@code {
    private bool shouldRender = true;

    protected override bool ShouldRender()
    {
        return shouldRender;
    }

    private void PreventRerender()
    {
        shouldRender = false;
        // Do work that shouldn't trigger re-render
        shouldRender = true;
    }
}
```

## StateHasChanged

Manually triggers a re-render. Needed when state changes outside of Blazor's event handlers.

```razor
@implements IDisposable
@inject INotificationService Notifications

@code {
    protected override void OnInitialized()
    {
        Notifications.OnMessage += HandleMessage;
    }

    private async void HandleMessage(string message)
    {
        messages.Add(message);
        
        // Must call StateHasChanged for external events
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Notifications.OnMessage -= HandleMessage;
    }
}
```

## IDisposable / IAsyncDisposable

Clean up resources when the component is removed.

```razor
@implements IAsyncDisposable
@inject IJSRuntime JS

@code {
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<MyComponent>? objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/myModule.js");
            
            objRef = DotNetObjectReference.Create(this);
            await jsModule.InvokeVoidAsync("initialize", objRef);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (jsModule is not null)
        {
            await jsModule.InvokeVoidAsync("cleanup");
            await jsModule.DisposeAsync();
        }
        
        objRef?.Dispose();
    }
}
```

## Lifecycle Diagram

```
Component Created
       ↓
SetParametersAsync
       ↓
OnInitialized(Async)  ← First time only
       ↓
OnParametersSet(Async) ← Every parameter update
       ↓
  BuildRenderTree
       ↓
OnAfterRender(Async)
       ↓
  [User Interaction]
       ↓
  StateHasChanged
       ↓
   ShouldRender?
    ↓ Yes          ↓ No
BuildRenderTree  Stop
       ↓
OnAfterRender(Async)
       .
       .
       ↓
  Dispose(Async)  ← Component removed
```

## Best Practices

- Use `OnInitializedAsync` for initial data loading
- Use `OnParametersSetAsync` when data depends on parameters
- Use `OnAfterRenderAsync(firstRender)` for JS interop
- Always implement `IDisposable` when subscribing to events
- Use `InvokeAsync(StateHasChanged)` for thread safety
- Avoid expensive operations in `ShouldRender`
