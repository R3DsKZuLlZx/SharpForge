namespace SharpForge.Client.Data;

/// <summary>
/// Static data for all course lessons used by LessonSidebar component.
/// </summary>
public static class CourseLessons
{
    public static readonly List<LessonInfo> CSharpFundamentals =
    [
        new(1, "Introduction to C# and .NET", "training/csharp-fundamentals/lesson/1"),
        new(2, "Your First C# Program", "training/csharp-fundamentals/lesson/2"),
        new(3, "Variables and Data Types", "training/csharp-fundamentals/lesson/3"),
        new(4, "Operators and Expressions", "training/csharp-fundamentals/lesson/4"),
        new(5, "Control Flow: Conditionals", "training/csharp-fundamentals/lesson/5"),
        new(6, "Control Flow: Loops", "training/csharp-fundamentals/lesson/6"),
        new(7, "Methods and Parameters", "training/csharp-fundamentals/lesson/7"),
        new(8, "Arrays and Collections", "training/csharp-fundamentals/lesson/8"),
        new(9, "Introduction to OOP: Classes", "training/csharp-fundamentals/lesson/9"),
        new(10, "OOP: Inheritance and Polymorphism", "training/csharp-fundamentals/lesson/10"),
        new(11, "Exception Handling", "training/csharp-fundamentals/lesson/11"),
        new(12, "LINQ Basics", "training/csharp-fundamentals/lesson/12")
    ];

    public static readonly List<LessonInfo> GettingStartedDotNet =
    [
        new(1, "What is .NET?", "training/getting-started-dotnet/lesson/1"),
        new(2, "Installing the .NET SDK", "training/getting-started-dotnet/lesson/2"),
        new(3, "The .NET CLI", "training/getting-started-dotnet/lesson/3"),
        new(4, "Project Structure and Files", "training/getting-started-dotnet/lesson/4"),
        new(5, "NuGet Package Management", "training/getting-started-dotnet/lesson/5"),
        new(6, "Building Console Applications", "training/getting-started-dotnet/lesson/6"),
        new(7, "Configuration and Settings", "training/getting-started-dotnet/lesson/7"),
        new(8, "Introduction to ASP.NET Core", "training/getting-started-dotnet/lesson/8"),
        new(9, "Debugging and Troubleshooting", "training/getting-started-dotnet/lesson/9"),
        new(10, "Publishing and Deployment", "training/getting-started-dotnet/lesson/10")
    ];

    public static readonly List<LessonInfo> AspNetCoreWebApis =
    [
        new(1, "Introduction to Web APIs", "training/aspnet-core-web-apis/lesson/1"),
        new(2, "Creating Your First API", "training/aspnet-core-web-apis/lesson/2"),
        new(3, "Routing and Endpoints", "training/aspnet-core-web-apis/lesson/3"),
        new(4, "Model Binding", "training/aspnet-core-web-apis/lesson/4"),
        new(5, "Validation", "training/aspnet-core-web-apis/lesson/5"),
        new(6, "Dependency Injection", "training/aspnet-core-web-apis/lesson/6"),
        new(7, "Entity Framework Core", "training/aspnet-core-web-apis/lesson/7"),
        new(8, "Repository Pattern", "training/aspnet-core-web-apis/lesson/8"),
        new(9, "Error Handling", "training/aspnet-core-web-apis/lesson/9"),
        new(10, "Authentication", "training/aspnet-core-web-apis/lesson/10"),
        new(11, "Authorization", "training/aspnet-core-web-apis/lesson/11"),
        new(12, "API Versioning", "training/aspnet-core-web-apis/lesson/12"),
        new(13, "Caching", "training/aspnet-core-web-apis/lesson/13"),
        new(14, "Rate Limiting", "training/aspnet-core-web-apis/lesson/14"),
        new(15, "OpenAPI/Swagger", "training/aspnet-core-web-apis/lesson/15"),
        new(16, "Testing APIs", "training/aspnet-core-web-apis/lesson/16"),
        new(17, "Logging and Monitoring", "training/aspnet-core-web-apis/lesson/17"),
        new(18, "Deployment", "training/aspnet-core-web-apis/lesson/18")
    ];

    public static readonly List<LessonInfo> BlazorWebAssembly =
    [
        new(1, "Introduction to Blazor", "training/blazor-webassembly/lesson/1"),
        new(2, "Components Basics", "training/blazor-webassembly/lesson/2"),
        new(3, "Data Binding", "training/blazor-webassembly/lesson/3"),
        new(4, "Event Handling", "training/blazor-webassembly/lesson/4"),
        new(5, "Component Parameters", "training/blazor-webassembly/lesson/5"),
        new(6, "Layouts and Routing", "training/blazor-webassembly/lesson/6"),
        new(7, "Forms and Validation", "training/blazor-webassembly/lesson/7"),
        new(8, "Dependency Injection", "training/blazor-webassembly/lesson/8"),
        new(9, "HTTP and API Calls", "training/blazor-webassembly/lesson/9"),
        new(10, "State Management", "training/blazor-webassembly/lesson/10"),
        new(11, "JavaScript Interop", "training/blazor-webassembly/lesson/11"),
        new(12, "Authentication", "training/blazor-webassembly/lesson/12"),
        new(13, "Performance", "training/blazor-webassembly/lesson/13"),
        new(14, "Deployment", "training/blazor-webassembly/lesson/14")
    ];

    public static readonly List<LessonInfo> EntityFrameworkCore =
    [
        new(1, "Introduction to EF Core", "training/entity-framework-core/lesson/1"),
        new(2, "DbContext and Models", "training/entity-framework-core/lesson/2"),
        new(3, "Migrations", "training/entity-framework-core/lesson/3"),
        new(4, "CRUD Operations", "training/entity-framework-core/lesson/4"),
        new(5, "Querying Data", "training/entity-framework-core/lesson/5"),
        new(6, "Relationships", "training/entity-framework-core/lesson/6"),
        new(7, "Fluent API", "training/entity-framework-core/lesson/7"),
        new(8, "Change Tracking", "training/entity-framework-core/lesson/8"),
        new(9, "Transactions", "training/entity-framework-core/lesson/9"),
        new(10, "Performance", "training/entity-framework-core/lesson/10"),
        new(11, "Raw SQL", "training/entity-framework-core/lesson/11"),
        new(12, "Testing with EF Core", "training/entity-framework-core/lesson/12")
    ];

    public static readonly List<LessonInfo> DesignPatterns =
    [
        new(1, "Introduction to Design Patterns", "training/design-patterns/lesson/1"),
        new(2, "Single Responsibility Principle", "training/design-patterns/lesson/2"),
        new(3, "Open/Closed Principle", "training/design-patterns/lesson/3"),
        new(4, "Liskov Substitution Principle", "training/design-patterns/lesson/4"),
        new(5, "Interface Segregation Principle", "training/design-patterns/lesson/5"),
        new(6, "Dependency Inversion Principle", "training/design-patterns/lesson/6"),
        new(7, "Singleton Pattern", "training/design-patterns/lesson/7"),
        new(8, "Factory Method Pattern", "training/design-patterns/lesson/8"),
        new(9, "Abstract Factory Pattern", "training/design-patterns/lesson/9"),
        new(10, "Builder Pattern", "training/design-patterns/lesson/10"),
        new(11, "Prototype Pattern", "training/design-patterns/lesson/11"),
        new(12, "Adapter Pattern", "training/design-patterns/lesson/12"),
        new(13, "Decorator Pattern", "training/design-patterns/lesson/13"),
        new(14, "Facade Pattern", "training/design-patterns/lesson/14"),
        new(15, "Proxy Pattern", "training/design-patterns/lesson/15"),
        new(16, "Composite Pattern", "training/design-patterns/lesson/16"),
        new(17, "Strategy Pattern", "training/design-patterns/lesson/17"),
        new(18, "Observer Pattern", "training/design-patterns/lesson/18"),
        new(19, "Command Pattern", "training/design-patterns/lesson/19"),
        new(20, "State Pattern", "training/design-patterns/lesson/20"),
        new(21, "Template Method Pattern", "training/design-patterns/lesson/21"),
        new(22, "Chain of Responsibility", "training/design-patterns/lesson/22"),
        new(23, "Clean Architecture", "training/design-patterns/lesson/23"),
        new(24, "Putting It All Together", "training/design-patterns/lesson/24")
    ];

    public static readonly List<LessonInfo> AsyncConcurrency =
    [
        new(1, "Introduction to Async", "training/async-concurrency/lesson/1"),
        new(2, "Tasks and Task<T>", "training/async-concurrency/lesson/2"),
        new(3, "Async/Await Keywords", "training/async-concurrency/lesson/3"),
        new(4, "Async Best Practices", "training/async-concurrency/lesson/4"),
        new(5, "Exception Handling", "training/async-concurrency/lesson/5"),
        new(6, "Cancellation Tokens", "training/async-concurrency/lesson/6"),
        new(7, "Progress Reporting", "training/async-concurrency/lesson/7"),
        new(8, "Parallel Programming", "training/async-concurrency/lesson/8"),
        new(9, "PLINQ", "training/async-concurrency/lesson/9"),
        new(10, "Concurrent Collections", "training/async-concurrency/lesson/10"),
        new(11, "Synchronization", "training/async-concurrency/lesson/11"),
        new(12, "Channels", "training/async-concurrency/lesson/12"),
        new(13, "Dataflow", "training/async-concurrency/lesson/13"),
        new(14, "ValueTask", "training/async-concurrency/lesson/14"),
        new(15, "Common Pitfalls", "training/async-concurrency/lesson/15"),
        new(16, "Debugging Async Code", "training/async-concurrency/lesson/16")
    ];

    public static readonly List<LessonInfo> Microservices =
    [
        new(1, "Introduction", "training/microservices/lesson/1"),
        new(2, "Monolith to Microservices", "training/microservices/lesson/2"),
        new(3, "DDD Basics", "training/microservices/lesson/3"),
        new(4, "Docker Fundamentals", "training/microservices/lesson/4"),
        new(5, "Dockerizing .NET", "training/microservices/lesson/5"),
        new(6, "Docker Compose", "training/microservices/lesson/6"),
        new(7, "HTTP Communication", "training/microservices/lesson/7"),
        new(8, "gRPC", "training/microservices/lesson/8"),
        new(9, "Async Messaging", "training/microservices/lesson/9"),
        new(10, "RabbitMQ", "training/microservices/lesson/10"),
        new(11, "Event Sourcing", "training/microservices/lesson/11"),
        new(12, "Database per Service", "training/microservices/lesson/12"),
        new(13, "Saga Pattern", "training/microservices/lesson/13"),
        new(14, "Outbox Pattern", "training/microservices/lesson/14"),
        new(15, "API Gateway", "training/microservices/lesson/15"),
        new(16, "YARP", "training/microservices/lesson/16"),
        new(17, "Authentication", "training/microservices/lesson/17"),
        new(18, "Kubernetes Basics", "training/microservices/lesson/18"),
        new(19, "Deploying to K8s", "training/microservices/lesson/19"),
        new(20, "ConfigMaps & Secrets", "training/microservices/lesson/20"),
        new(21, "Health Checks", "training/microservices/lesson/21"),
        new(22, "Scaling", "training/microservices/lesson/22"),
        new(23, "Centralized Logging", "training/microservices/lesson/23"),
        new(24, "Distributed Tracing", "training/microservices/lesson/24"),
        new(25, "Metrics", "training/microservices/lesson/25"),
        new(26, "Resilience Patterns", "training/microservices/lesson/26"),
        new(27, "Polly", "training/microservices/lesson/27"),
        new(28, "Complete System", "training/microservices/lesson/28")
    ];

    public record LessonInfo(int Number, string Title, string Url);
}


