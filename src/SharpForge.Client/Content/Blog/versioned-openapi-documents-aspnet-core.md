---
title: "Versioned OpenAPI Documents Without the Boilerplate"
category: "ASP.NET Core"
date: "October 6, 2026"
readTime: "9 min read"
excerpt: "Microsoft.AspNetCore.OpenApi has no idea what an API version is. Asp.Versioning.OpenApi fixes that - one document per version, discovered from your attributes, shaped for API Management."
tags: ["OpenAPI", "API Versioning", "ASP.NET Core", "Azure API Management"]
sidebar:
  - href: "#why-this-needs-a-package-at-all"
    text: "Why a Package?"
  - href: "#the-setup"
    text: "The Setup"
  - href: "#one-document-per-version-for-free"
    text: "One Document per Version"
  - href: "#taking-the-version-out-of-the-paths"
    text: "Version-Neutral Paths"
  - href: "#the-group-name-format-has-to-match"
    text: "Group Name Format"
  - href: "#generating-documents-at-build-time"
    text: "Build-Time Generation"
  - href: "#making-the-transformer-reusable"
    text: "Reusable Transformer"
  - href: "#when-not-to-use-this"
    text: "When Not to Use This"
  - href: "#conclusion"
    text: "Conclusion"
---

Since .NET 9 the Web API template ships with `Microsoft.AspNetCore.OpenApi` instead of Swashbuckle. Generating documents is now built in, but versioning isn't, you register every document by name, and nothing stops `AddOpenApi("v3")` from drifting away from the `[ApiVersion(3)]` attributes it is supposed to describe. `Asp.Versioning.OpenApi` closes that gap. Your controllers declare their versions, and one OpenAPI document per version appears on its own. My take is that this should be the default for any URL-versioned API on the built-in stack.

## Why This Needs a Package At All

Without versioning integration, the built-in package makes you list each document by hand:

```csharp
// ❌ BAD: every new version is a change in two places
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");
builder.Services.AddOpenApi("v3");
```

That list has to match your controllers. Add `[ApiVersion(4)]` and forget the fourth line, and v4 ships with no documentation and no error. Remove a version and forget the line, and you publish an empty document. Either way, you are keeping two lists in sync by hand.

`Asp.Versioning.OpenApi` makes the API explorer the single source of truth. It already knows every version your controllers declare, so it creates a document for each one.

## The Setup

The proof of concept is a .NET 10 controller API using URL-segment versioning. The full source is on [GitHub](https://github.com/R3DsKZuLlZx/OpenApiVersioning/tree/main). Here are the packages:

```xml
<ItemGroup>
  <PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="10.2.1" />
  <PackageReference Include="Asp.Versioning.OpenApi" Version="10.2.3" />
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.12" />
  <PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.2.3" />
</ItemGroup>
```

Only the UI package comes from Swashbuckle. `Microsoft.AspNetCore.OpenApi` generates the documents, and `Swashbuckle.AspNetCore.SwaggerUI` just renders them. This can then easily be switched out with another package like `Scalar.AspNetCore`.

The controller declares three versions and maps each action to one of them:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/weather")]
[ApiVersion(1)]
[ApiVersion(2)]
[ApiVersion(3)]
public class WeatherController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion(1)]
    public IActionResult GetV1() => Ok("v1");

    [HttpGet]
    [MapToApiVersion(2)]
    public IActionResult GetV2() => Ok("v2");

    [HttpPost]
    [MapToApiVersion(3)]
    public IActionResult PostV3() => Ok("v3");
}
```

## One Document per Version, for Free

The registration chains off `AddApiVersioning`. The `AddOpenApi` call at the end is the versioning-aware one from `Asp.Versioning.OpenApi`, not the one on `IServiceCollection`:

```csharp
builder.Services.AddControllers();

builder.Services
    .AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    })
    .AddOpenApi(options =>
    {
        options.Document.AddDocumentTransformer((document, context, _) =>
        {
            // shown in the next section
            return Task.CompletedTask;
        });
    });
```

You map the endpoints once with `WithDocumentPerVersion()`. Swagger UI builds its dropdown from `DescribeApiVersions()`, so it can't drift from the documents either:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();

    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint(
                $"/openapi/{description.GroupName}.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}
```

That's all the wiring. `/openapi/v1.json` and `/openapi/v2.json` each describe a single `GET`. `/openapi/v3.json` describes only the `POST`. Adding `[ApiVersion(4)]` to a controller gives you a fourth document and a fourth dropdown entry, and `Program.cs` doesn't change.

## Taking the Version Out of the Paths

By default, each document lists its operations under their full route, such as `/api/v2/weather`. That works fine in Swagger UI. It's the wrong shape for **Azure API Management**.

APIM has its own versioning model. You import each document into a version set, and with the path scheme APIM adds the version segment itself. If the version is also baked into every operation path, the version ends up in two places. The document should describe version-neutral operations, with the version in the base URL.

The document transformer does that. It moves `/api/{version}` into `servers` and strips it from every path:

```csharp
options.Document.AddDocumentTransformer((document, context, _) =>
{
    document.Info.Title = "Weather API";
    document.Info.Contact = new OpenApiContact
    {
        Name = "Platform Team",
        Email = "platform@example.com"
    };

    var prefix = $"/api/{context.DocumentName}";

    document.Servers = new List<OpenApiServer>
    {
        new() { Url = prefix, Description = $"Version {context.DocumentName}" }
    };

    var paths = new OpenApiPaths();
    foreach (var (key, value) in document.Paths)
    {
        paths.Add(key.Replace(prefix, string.Empty, StringComparison.OrdinalIgnoreCase), value);
    }

    document.Paths = paths;
    return Task.CompletedTask;
});
```

`context.DocumentName` is the API explorer group name, `v2` here. One transformer serves every version because the prefix is built per document. The generated v2 document comes out like this:

```json
{
  "openapi": "3.1.1",
  "info": {
    "title": "Weather API",
    "version": "2.0"
  },
  "servers": [
    { "url": "/api/v2", "description": "Version v2" }
  ],
  "paths": {
    "/weather": {
      "get": {
        "tags": ["Weather"],
        "responses": { "200": { "description": "OK" } }
      }
    }
  }
}
```

The operation is `/weather`, and the version lives only in `servers`. That's the shape APIM's version sets want.

## The Group Name Format Has to Match

This is the one real gotcha. The transformer assumes the **document name** is the same string as the **URL segment**. Both come from the API explorer:

- `GroupNameFormat = "'v'V"` names the documents `v1`, `v2`, `v3`. The quoted `'v'` is a literal, and `V` is the major version.
- `SubstituteApiVersionInUrl = true` turns `api/v{version:apiVersion}` into `api/v2` in the documented paths.

If those two drift apart, nothing throws. Change the format to one that renders differently from the route segment, and `prefix` stops matching. `Replace` silently does nothing, and you get documents where `servers` says `/api/v2` and the paths still say `/api/v2/weather`. Leave out `SubstituteApiVersionInUrl` and the paths keep a literal `{version}` placeholder, which also won't match.

The Swagger UI endpoints and the build-time file names use the same group name, so a format change affects all three. Pick a format that renders exactly like your route segment, and test it. The paths in the generated document are the thing to assert on.

## Generating Documents at Build Time

Add `Microsoft.Extensions.ApiDescription.Server` and two MSBuild properties, and the documents get written to disk on every build:

```xml
<PropertyGroup>
  <OpenApiGenerateDocumentsOnBuild>true</OpenApiGenerateDocumentsOnBuild>
  <OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)</OpenApiDocumentsDirectory>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.ApiDescription.Server" Version="10.0.12">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

You get one file per version, named after the project and the group, such as `OpenApiVersioning.Web_v2.json`. Commit them, diff them in pull requests, and have your pipeline import them into APIM. An unexpected API change then shows up in code review, not after release.

One thing to watch: build-time documents are generated by a separate tool, not by your running app. In the proof of concept, `info.description` came out as `"GetDocument Command-line Tool inside man"`, which is the generator tool's own description leaking through. Set `document.Info.Description` explicitly in the transformer so the description you publish to APIM is one you wrote.

## Making the Transformer Reusable

The inline lambda is fine for a proof of concept. Across several services, move it into an `IOpenApiDocumentTransformer` so every API applies the same rule:

```csharp
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

public sealed class UrlSegmentVersionTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var prefix = $"/api/{context.DocumentName}";

        document.Servers = [new OpenApiServer { Url = prefix, Description = $"Version {context.DocumentName}" }];

        var paths = new OpenApiPaths();
        foreach (var (key, value) in document.Paths)
        {
            var path = key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? key[prefix.Length..]
                : key;

            paths.Add(path.Length == 0 ? "/" : path, value);
        }

        document.Paths = paths;
        return Task.CompletedTask;
    }
}
```

```csharp
.AddOpenApi(options => options.Document.AddDocumentTransformer<UrlSegmentVersionTransformer>());
```

Two small hardening changes are in there. First, `StartsWith` plus slicing only strips the leading prefix, while `Replace` would remove a match anywhere in the path. Second, a route that is exactly the prefix becomes `/` instead of an empty key. The `/api` root is still hard-coded. If your services use different roots, pass it in through options.

## When Not to Use This

- **Header, query-string or media-type versioning.** The transformer only makes sense when the version is in the URL. With other readers, there's no path segment to move into `servers`. The document-per-version discovery still works, but you don't need the path rewriting.
- **You aren't putting the API behind a gateway.** If clients call the service directly and Swagger UI is the only consumer, full paths are easier to read. Keep the discovery and drop the transformer.
- **One version, no plans for another.** Asp.Versioning adds three packages and a set of conventions. For an internal API that will only ever have v1, plain `AddOpenApi()` is simpler.
- **You're happy on Swashbuckle or NSwag.** Both already have versioning integrations. Switching generators is a separate decision.

## Conclusion

Built-in OpenAPI plus `Asp.Versioning.OpenApi` gives you versioned documents without a hand-written list to keep in sync. The controllers say which versions exist, and the documents, Swagger UI entries and build output follow.

- **Let attributes drive documents.** `[ApiVersion]` is the only place a version should be declared.
- **Keep `GroupNameFormat` and the route segment identical.** The transformer depends on it, and it fails silently when they differ.
- **Keep operation paths version-neutral for APIM.** Put the version in `servers` and let the version set do the routing.
- **Generate at build time and commit the output.** Then API changes get reviewed as diffs.
- **Set `Info.Description` yourself.** Otherwise the build tool fills it in for you.

