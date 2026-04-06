namespace SharpForge.Client.Data;

/// <summary>
/// Central registry that maps a course slug to its metadata.
/// Used by <see cref="Components.LessonLayout"/> to derive sidebar,
/// header, and footer values from just a slug and lesson number.
/// </summary>
public static class CourseRegistry
{
    public static readonly Dictionary<string, CourseMetadata> Courses = new()
    {
        ["csharp-fundamentals"] = new("C# Fundamentals", "Beginner", CourseLessons.CSharpFundamentals),
        ["getting-started-dotnet"] = new("Getting Started with .NET", "Beginner", CourseLessons.GettingStartedDotNet),
        ["aspnet-core-web-apis"] = new("ASP.NET Core Web APIs", "Intermediate", CourseLessons.AspNetCoreWebApis),
        ["blazor-webassembly"] = new("Blazor WebAssembly", "Intermediate", CourseLessons.BlazorWebAssembly),
        ["entity-framework-core"] = new("Entity Framework Core", "Intermediate", CourseLessons.EntityFrameworkCore),
        ["design-patterns"] = new("Design Patterns in C#", "Advanced", CourseLessons.DesignPatterns),
        ["async-concurrency"] = new("Async/Await & Concurrency", "Advanced", CourseLessons.AsyncConcurrency),
        ["microservices"] = new("Microservices with .NET", "Advanced", CourseLessons.Microservices),
    };

    public record CourseMetadata(string Title, string Level, List<CourseLessons.LessonInfo> Lessons);
}
