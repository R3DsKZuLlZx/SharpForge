---
title: "Building Result Types with C# Unions in .NET 11"
category: "C#"
date: "October 10, 2026"
readTime: "8 min read"
excerpt: "C# unions let the compiler check that every outcome of a call is handled. Learn how to build a Result type with the new union keyword, and where it falls short."
tags: ["C#", "Unions", "Result Type", ".NET 11"]
sidebar:
  - href: "#the-problem-with-hand-rolled-result-types"
    text: "The Problem"
  - href: "#what-a-union-is"
    text: "What a Union Is"
  - href: "#a-result-in-one-line"
    text: "A Result in One Line"
  - href: "#handling-every-outcome"
    text: "Handling Every Outcome"
  - href: "#when-the-generated-union-is-not-enough"
    text: "Custom Unions"
  - href: "#costs-and-caveats"
    text: "Costs and Caveats"
  - href: "#conclusion"
    text: "Conclusion"
---

Most .NET code reports failure with exceptions, and most hand-rolled alternatives reach for a base class, a marker interface or a bag of nullable properties. None of them let the compiler tell you that you forgot a case. C# unions, documented for C# 15 and .NET 11, fix that: you declare the closed set of outcomes once, and every `switch` over it is checked.

## The Problem with Hand-Rolled Result Types

A typical home-grown result looks like this:

```csharp
// ❌ BAD: nothing forces the caller to look at Error
public class ImportResult
{
    public Invoice? Invoice { get; init; }
    public string? Error { get; init; }
}

var result = importer.Import(file);
Console.WriteLine(result.Invoice!.Number); // compiles, throws when Import failed
```

Both properties are optional, so the type allows states that should not exist (both set, neither set). The alternative, an abstract base class with `Success` and `Failure` subclasses, is safer but needs ceremony, and a `switch` over it still needs a `_ =>` arm that hides any case you add later.

## What a Union Is

A **union type** represents a value that is one of several **case types**. The `union` keyword declares it:

```csharp
public union Pet(Cat, Dog, Bird);
```

Three things come with it, according to the Microsoft Learn documentation:

- An implicit conversion from each case type to the union.
- Exhaustive pattern matching: a `switch` expression that misses a case type produces a warning, and you don't need a discard arm.
- Enhanced nullability tracking for the union's contents.

A union doesn't define new data members, and unlike an `interface` it is closed: the compiler uses the list in the declaration to check exhaustiveness. Unlike a `record`, it adds no equality, cloning or deconstruction behaviour. It answers "which case is it?" and nothing else.

Case types can be classes, structs, interfaces, type parameters, nullable types, or other unions. The compiler turns the declaration into a struct marked with `[Union]` that implements `IUnion` and has a `Value` property of type `object?`.

## A Result in One Line

The documentation lists result-or-error returns as a primary scenario, so a result type is a natural first use. The case types are ordinary records, and the union is a single declaration:

```csharp
public record class Ok<T>(T Value);
public record class Failure(string Code, string Message);

public union Result<T>(Ok<T>, Failure);
```

The docs show a generic union of the same shape, `union Option<T>(None, Some<T>)`, so type parameters in case types are supported. Producing a result needs no factory methods, because each case type converts implicitly:

```csharp
public Result<Invoice> Import(Stream file)
{
    if (file.Length == 0)
        return new Failure("empty-file", "The uploaded file contains no data.");

    var invoice = Parse(file);
    return new Ok<Invoice>(invoice);
}
```

Compare that with the base-class version: no `abstract` type, no private constructors, no static `Success()` and `Failure()` helpers. The conversion works by calling the constructor the compiler generates for each case type.

## Handling Every Outcome

Pattern matching "unwraps" the union. Patterns apply to the union's `Value`, not to the union itself, so you match directly on the case types:

```csharp
IResult ToHttpResult(Result<Invoice> result) => result switch
{
    Ok<Invoice> ok            => Results.Ok(ok.Value),
    Failure { Code: "empty-file" } f => Results.BadRequest(f.Message),
    Failure f                 => Results.Problem(f.Message),
};
```

There's no discard arm. If you later add a third case type to `Result<T>`, such as `Pending`, every `switch` like this one produces a compiler warning until it handles the new case. That is the safety gain over the base-class design, where the `_ =>` arm silently absorbs the new type.

One trap: because patterns target `Value`, `result is Result<Invoice>` typically doesn't match. You are testing the contents, not the wrapper.

### The null arm

The generated union is a struct, so `default` is legal, and a defaulted union has a `Value` of `null`. The docs state that when the null state of `Value` is "maybe null", you must also handle `null` or get a warning:

```csharp
Result<Invoice> result = default;

var message = result switch
{
    Ok<Invoice> ok => $"Imported {ok.Value.Number}",
    Failure f      => f.Message,
    null           => "No result was produced.",
};
```

When the union is definitely assigned from a case type, you don't need the `null` arm. Treat a `null` arm on a `default` union as a bug report, not a normal outcome.

## When the Generated Union Is Not Enough

The generated form is opinionated: always a struct, always boxing value-type cases, always an `object?` field. You can write a union by hand instead. Any class or struct with a `[Union]` attribute is a union type if it has one or more public single-parameter constructors (each parameter type defines a case type) and a public `Value` property of type `object?` or `object`.

The documentation's class-based example is a result type with a `string` or an `Exception` as the cases:

```csharp
[System.Runtime.CompilerServices.Union]
public class Result<T> : System.Runtime.CompilerServices.IUnion
{
    private readonly object? _value;

    public Result(T? value) { _value = value; }
    public Result(Exception? value) { _value = value; }

    public object? Value => _value;
}
```

Consumers match as before, with one difference for class-based unions: the `null` pattern succeeds when either the union reference or its `Value` is null.

```csharp
static string Describe(Result<string> result) => result switch
{
    string s    => $"OK: {s}",
    Exception e => $"Error: {e.Message}",
    null        => "null",
};
```

Choose this form when you need reference semantics or inheritance. The compiler relies on some behavioural rules you must uphold: `Value` returns only `null` or one of the case types, and a value created from a case type reports that same case back.

For value-type cases, a custom union can also implement the **non-boxing access pattern**: a `HasValue` property plus a `TryGetValue(out T value)` method per case type. The compiler then uses `TryGetValue` for type patterns and `HasValue` for null patterns instead of reading `Value`, which avoids boxing.

## Costs and Caveats

- **It's new.** The documentation describes the `Union` attribute and `IUnion` as included in the runtime beginning with .NET 11 Preview 5, and the language feature specification lives under C# 15. Check the current status of your SDK before adopting unions in production code. This post does not cover how to enable them.
- **Boxing.** A generated union stores its contents as one `object?`, so value-type cases are boxed. On a hot path, measure it, or write a custom union with the non-boxing access pattern. This post has no benchmarks, so measure before deciding.
- **No payload sharing.** A union adds no members of its own. Shared data belongs on the case types, or in members you add to the union's body. Those bodies can't declare instance fields or auto-properties.
- **Warnings, not errors.** Missing cases are reported as warnings. If you want the guarantee to be enforced, treat that warning as an error in your build configuration.
- **Not a replacement for exceptions.** Use a result type for expected outcomes the caller must decide about, such as validation or a missing record. Keep exceptions for the unexpected.

## Conclusion

A C# union is the missing piece that makes result types worth writing: one declaration lists the outcomes, implicit conversions remove the factory boilerplate, and exhaustive `switch` expressions turn "I forgot the failure path" into a compiler warning. Start with the one-line `union Result<T>(Ok<T>, Failure)` form, handle `null` only where a `default` union can reach you, and reach for a hand-written union only when you need a class or non-boxing storage. Because the feature is tied to .NET 11 previews in the documentation, try it on a branch first and promote it when your toolchain supports it.
