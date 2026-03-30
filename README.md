# SharpForge

[![Deploy to GitHub Pages](https://github.com/R3DsKZuLlZx/SharpForge/actions/workflows/deploy.yml/badge.svg)](https://github.com/R3DsKZuLlZx/SharpForge/actions/workflows/deploy.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-blueviolet)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A sample Blazor client application and training site with example blog posts, course content, and reusable components.

SharpForge is designed as a learning/reference project demonstrating modern Blazor patterns, client-side components, and small training lessons you can adapt for your own tutorials.

## About

This repository contains a single Blazor client application located at `src/SharpForge.Client`. It includes:
- A small collection of reusable UI components (buttons, layouts, code highlighter, theme toggle).
- Example pages for blog posts, courses, and training material.
- Services for theme and course progress.

## Key features

- Blazor client application targeting .NET 10.
- Reusable Razor components organized under `Components/`.
- Sample blog and course content under `Pages/` and `Pages/Blogs/`.
- Simple client-side services for theming and course progress tracking.

## Technology

- .NET 10
- Blazor

## Getting started

Clone the repository:

```powershell
git clone https://github.com/R3DsKZuLlZx/SharpForge.git
cd SharpForge
```

Build the solution:

```powershell
dotnet build
```

Run the Blazor client app:

```powershell
dotnet run --project src\SharpForge.Client\SharpForge.Client.csproj
```
