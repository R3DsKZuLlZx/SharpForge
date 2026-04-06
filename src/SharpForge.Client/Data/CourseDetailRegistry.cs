using SharpForge.Client.Models;

namespace SharpForge.Client.Data;

/// <summary>
/// Central registry of course detail pages, keyed by slug.
/// Consumed by the generic <c>Course.razor</c> page.
/// </summary>
public static class CourseDetailRegistry
{
    private static Dictionary<string, CourseDetail>? _courses;

    public static Dictionary<string, CourseDetail> Courses => _courses ??= new()
    {
        ["csharp-fundamentals"] = CSharpFundamentals,
        ["getting-started-dotnet"] = GettingStartedDotNet,
        ["aspnet-core-web-apis"] = AspNetCoreWebApis,
        ["blazor-webassembly"] = BlazorWebAssembly,
        ["entity-framework-core"] = EntityFrameworkCore,
        ["design-patterns"] = DesignPatterns,
        ["async-concurrency"] = AsyncConcurrency,
        ["microservices"] = Microservices,
    };

    // ───────────────────────── Beginner ─────────────────────────

    private static readonly CourseDetail CSharpFundamentals = new()
    {
        Slug = "csharp-fundamentals",
        Title = "C# Fundamentals",
        Description = "Master the fundamentals of C# programming. Learn syntax, data types, control flow, object-oriented programming, and build your first applications.",
        Level = CourseLevel.Beginner,
        Duration = "3 Hours",
        StudentCount = "59",
        ExerciseCount = 24,
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#csharp-gradient)"/>
                <text x="64" y="78" text-anchor="middle" font-family="Inter, Arial, sans-serif" font-size="40" font-weight="700" fill="white">C#</text>
                <defs>
                    <linearGradient id="csharp-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#68217A"/>
                        <stop offset="1" stop-color="#9B4DCA"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Understand C# syntax and programming concepts",
            "Work with variables, data types, and operators",
            "Use control flow statements (if, switch, loops)",
            "Create and use methods effectively",
            "Master object-oriented programming principles",
            "Work with collections and LINQ basics",
            "Handle exceptions and errors properly",
            "Build your first console applications",
        ],
        Prerequisites =
        [
            "No prior programming experience required",
            "Basic computer skills",
            "A computer with Windows, macOS, or Linux",
            "Enthusiasm to learn!",
        ],
        Lessons =
        [
            new(1, "Introduction to C# and .NET", "Learn what C# is, its history, and how to set up your development environment.", "8 min"),
            new(2, "Your First C# Program", "Write your first 'Hello World' program and understand the basic structure of C# code.", "12 min"),
            new(3, "Variables and Data Types", "Explore built-in data types, declare variables, and understand type safety.", "12 min"),
            new(4, "Operators and Expressions", "Master arithmetic, comparison, logical, and assignment operators.", "10 min"),
            new(5, "Control Flow: Conditionals", "Learn if statements, else clauses, switch expressions, and ternary operators.", "12 min"),
            new(6, "Control Flow: Loops", "Master for, while, do-while, and foreach loops for repetitive tasks.", "10 min"),
            new(7, "Methods and Parameters", "Create reusable code with methods, parameters, return values, and overloading.", "12 min"),
            new(8, "Arrays and Collections", "Work with arrays, lists, dictionaries, and other collection types.", "12 min"),
            new(9, "Introduction to OOP: Classes", "Understand classes, objects, fields, properties, and constructors.", "15 min"),
            new(10, "OOP: Inheritance and Polymorphism", "Learn inheritance, virtual methods, overriding, and polymorphic behavior.", "12 min"),
            new(11, "Exception Handling", "Handle errors gracefully with try-catch-finally and custom exceptions.", "10 min"),
            new(12, "LINQ Basics", "Query collections efficiently with Language Integrated Query.", "20 min"),
        ],
    };

    private static readonly CourseDetail GettingStartedDotNet = new()
    {
        Slug = "getting-started-dotnet",
        Title = "Getting Started with .NET",
        Description = "Learn the fundamentals of the .NET platform. Understand the runtime, SDK, project structure, and build your first applications across web, console, and API projects.",
        Level = CourseLevel.Beginner,
        Duration = "2 Hours",
        StudentCount = "23",
        ExerciseCount = 20,
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#dotnet-gradient)"/>
                <text x="64" y="78" text-anchor="middle" font-family="Inter, Arial, sans-serif" font-size="32" font-weight="700" fill="white">.NET</text>
                <defs>
                    <linearGradient id="dotnet-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#512BD4"/>
                        <stop offset="1" stop-color="#7B68EE"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Understand the .NET ecosystem and its components",
            "Install and configure the .NET SDK",
            "Use the .NET CLI effectively",
            "Understand project structure and configuration",
            "Work with NuGet packages and dependencies",
            "Build console, web, and API applications",
            "Debug and troubleshoot .NET applications",
            "Deploy your applications to production",
        ],
        Prerequisites =
        [
            "Basic understanding of programming concepts (helpful but not required)",
            "A computer with Windows, macOS, or Linux",
            "Internet connection for downloading tools",
            "Willingness to learn and experiment!",
        ],
        Lessons =
        [
            new(1, "What is .NET?", "Understand the .NET platform, its history, and the different versions and implementations.", "8 min"),
            new(2, "Installing the .NET SDK", "Download, install, and verify your .NET development environment on any operating system.", "8 min"),
            new(3, "The .NET CLI", "Master the command-line interface for creating, building, running, and publishing .NET projects.", "12 min"),
            new(4, "Project Structure and Files", "Understand .csproj files, solution files, and how .NET projects are organized.", "10 min"),
            new(5, "NuGet Package Management", "Learn to find, install, update, and manage third-party packages in your projects.", "10 min"),
            new(6, "Building Console Applications", "Create interactive console apps with user input, output formatting, and command-line arguments.", "12 min"),
            new(7, "Configuration and Settings", "Work with appsettings.json, environment variables, and the Options pattern.", "10 min"),
            new(8, "Introduction to ASP.NET Core", "Get started with web development using ASP.NET Core and understand the request pipeline.", "15 min"),
            new(9, "Debugging and Troubleshooting", "Use Visual Studio and VS Code debugging tools to find and fix issues in your code.", "10 min"),
            new(10, "Publishing and Deployment", "Package your application for deployment and understand the different publishing options.", "15 min"),
        ],
    };

    // ───────────────────────── Intermediate ─────────────────────────

    private static readonly CourseDetail AspNetCoreWebApis = new()
    {
        Slug = "aspnet-core-web-apis",
        Title = "ASP.NET Core Web APIs",
        Description = "Build production-ready REST APIs with ASP.NET Core. Master routing, middleware, dependency injection, authentication, Entity Framework Core, and deployment strategies.",
        Level = CourseLevel.Intermediate,
        Duration = "3 hours",
        StudentCount = "47",
        ExerciseCount = 10,
        ProjectDescription = "Complete API project",
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#api-gradient)"/>
                <path d="M40 50h48M40 64h48M40 78h32" stroke="white" stroke-width="4" stroke-linecap="round"/>
                <circle cx="88" cy="78" r="6" fill="white"/>
                <defs>
                    <linearGradient id="api-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#0d6efd"/>
                        <stop offset="1" stop-color="#6610f2"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Build RESTful APIs following best practices",
            "Implement routing with controllers and minimal APIs",
            "Master middleware and the request pipeline",
            "Use dependency injection effectively",
            "Implement JWT authentication and authorization",
            "Work with Entity Framework Core for data access",
            "Handle errors and implement logging",
            "Deploy APIs with Docker to cloud platforms",
        ],
        Prerequisites =
        [
            "Solid understanding of C# fundamentals",
            "Basic knowledge of HTTP and REST concepts",
            "Familiarity with Visual Studio or VS Code",
            "Understanding of JSON data format",
        ],
        Lessons =
        [
            new(1, "Introduction to ASP.NET Core", "Understand the ASP.NET Core framework, its architecture, and how it differs from ASP.NET Framework.", "8 min"),
            new(2, "Creating Your First API", "Set up a new Web API project and create your first endpoint using controllers.", "12 min"),
            new(3, "Routing Fundamentals", "Master attribute routing, route templates, and route constraints.", "10 min"),
            new(4, "Controllers and Actions", "Learn about controller conventions, action methods, and return types.", "12 min"),
            new(5, "Model Binding and Validation", "Bind request data to models and implement validation with Data Annotations and FluentValidation.", "12 min"),
            new(6, "Dependency Injection", "Understand the built-in DI container, service lifetimes, and best practices.", "8 min"),
            new(7, "Middleware Pipeline", "Create custom middleware and understand the request/response pipeline.", "8 min"),
            new(8, "Entity Framework Core Basics", "Set up EF Core, create models, and perform CRUD operations.", "18 min"),
            new(9, "Repository Pattern", "Implement the repository pattern for cleaner data access code.", "10 min"),
            new(10, "Authentication with JWT", "Implement JSON Web Token authentication for securing your APIs.", "6 min"),
            new(11, "Authorization Policies", "Create role-based and policy-based authorization rules.", "8 min"),
            new(12, "Error Handling", "Implement global exception handling and return consistent error responses.", "6 min"),
            new(13, "Logging and Monitoring", "Add structured logging with Serilog and monitor your API health.", "8 min"),
            new(14, "API Versioning", "Implement URL, query string, and header-based API versioning.", "6 min"),
            new(15, "Swagger and OpenAPI", "Document your API with Swagger UI and OpenAPI specifications.", "6 min"),
            new(16, "Minimal APIs", "Build lightweight APIs using the minimal API approach in .NET 6+.", "10 min"),
            new(17, "Testing Your APIs", "Write unit tests and integration tests for your API endpoints.", "12 min"),
            new(18, "Deployment and Docker", "Deploy your API to Azure App Service and containerize with Docker.", "15 min"),
        ],
    };

    private static readonly CourseDetail BlazorWebAssembly = new()
    {
        Slug = "blazor-webassembly",
        Title = "Blazor WebAssembly",
        Description = "Create interactive web applications with Blazor WebAssembly. Build single-page applications using C# instead of JavaScript, with components, data binding, routing, and state management.",
        Level = CourseLevel.Intermediate,
        Duration = "3 Hours",
        StudentCount = "92",
        ExerciseCount = 8,
        ProjectDescription = "Complete Blazor app project",
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#blazor-gradient)"/>
                <path d="M44 44h40v40H44z" fill="none" stroke="white" stroke-width="4"/>
                <path d="M54 54h20v20H54z" fill="white"/>
                <circle cx="64" cy="38" r="6" fill="white"/>
                <circle cx="64" cy="90" r="6" fill="white"/>
                <circle cx="38" cy="64" r="6" fill="white"/>
                <circle cx="90" cy="64" r="6" fill="white"/>
                <defs>
                    <linearGradient id="blazor-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#512BD4"/>
                        <stop offset="1" stop-color="#9B4DCA"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Build interactive web UIs with C# instead of JavaScript",
            "Create reusable Razor components",
            "Implement data binding and event handling",
            "Configure routing and navigation",
            "Build and validate forms",
            "Call REST APIs from Blazor",
            "Manage application state effectively",
            "Deploy to Azure Static Web Apps and GitHub Pages",
        ],
        Prerequisites =
        [
            "Solid understanding of C# fundamentals",
            "Basic knowledge of HTML and CSS",
            "Familiarity with web development concepts",
            "Understanding of async/await patterns",
        ],
        Lessons =
        [
            new(1, "Introduction to Blazor", "Understand what Blazor is, the different hosting models, and when to use Blazor WebAssembly.", "10 min"),
            new(2, "Project Setup", "Create your first Blazor WebAssembly project and understand the project structure.", "10 min"),
            new(3, "Razor Components", "Learn the anatomy of Razor components, including markup, code, and styling.", "10 min"),
            new(4, "Component Parameters", "Pass data to components using parameters and cascading values.", "8 min"),
            new(5, "Event Handling", "Handle DOM events and create custom component events with EventCallback.", "10 min"),
            new(6, "Data Binding", "Implement one-way and two-way data binding with bind directives.", "10 min"),
            new(7, "Component Lifecycle", "Master OnInitialized, OnParametersSet, OnAfterRender, and disposal.", "10 min"),
            new(8, "Routing and Navigation", "Configure routes, route parameters, and programmatic navigation.", "8 min"),
            new(9, "Forms and Validation", "Build forms with validation using EditForm and data annotations.", "10 min"),
            new(10, "Calling REST APIs", "Use HttpClient to call APIs and handle responses in Blazor.", "10 min"),
            new(11, "State Management", "Manage state with cascading values, services, and browser storage.", "12 min"),
            new(12, "JavaScript Interop", "Call JavaScript from C# and vice versa when needed.", "8 min"),
            new(13, "Authentication", "Implement authentication with ASP.NET Core Identity and JWT.", "12 min"),
            new(14, "Deployment", "Deploy your Blazor WebAssembly app to Azure Static Web Apps and GitHub Pages.", "12 min"),
        ],
    };

    private static readonly CourseDetail EntityFrameworkCore = new()
    {
        Slug = "entity-framework-core",
        Title = "Entity Framework Core",
        Description = "Master database access with Entity Framework Core. Learn about DbContext, migrations, relationships, LINQ queries, and performance optimization for production applications.",
        Level = CourseLevel.Intermediate,
        Duration = "4 Hours",
        StudentCount = "56",
        ExerciseCount = 6,
        ProjectDescription = "Complete data layer project",
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#ef-gradient)"/>
                <ellipse cx="64" cy="50" rx="30" ry="12" stroke="white" stroke-width="4" fill="none"/>
                <path d="M34 50v28c0 6.627 13.431 12 30 12s30-5.373 30-12V50" stroke="white" stroke-width="4" fill="none"/>
                <path d="M34 64c0 6.627 13.431 12 30 12s30-5.373 30-12" stroke="white" stroke-width="4" fill="none"/>
                <defs>
                    <linearGradient id="ef-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#68217A"/>
                        <stop offset="1" stop-color="#512BD4"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Configure DbContext and database providers",
            "Create entity models with conventions and Fluent API",
            "Manage database schema with migrations",
            "Perform CRUD operations efficiently",
            "Write optimized LINQ queries",
            "Configure entity relationships",
            "Handle concurrency and transactions",
            "Optimize performance for production",
        ],
        Prerequisites =
        [
            "Solid understanding of C# fundamentals",
            "Basic knowledge of relational databases and SQL",
            "Familiarity with LINQ",
            "Understanding of object-oriented programming",
        ],
        Lessons =
        [
            new(1, "Introduction to EF Core", "Understand what Entity Framework Core is, its architecture, and how it compares to other ORMs.", "16 min"),
            new(2, "Setting Up DbContext", "Create and configure your DbContext, connection strings, and database providers.", "18 min"),
            new(3, "Defining Entity Models", "Create entity classes with proper conventions, data annotations, and Fluent API configuration.", "18 min"),
            new(4, "Migrations", "Create, apply, and manage database migrations for schema changes.", "16 min"),
            new(5, "Basic CRUD Operations", "Perform Create, Read, Update, and Delete operations with EF Core.", "18 min"),
            new(6, "Querying with LINQ", "Write efficient LINQ queries including filtering, sorting, grouping, and projections.", "21 min"),
            new(7, "Relationships", "Configure one-to-one, one-to-many, and many-to-many relationships between entities.", "21 min"),
            new(8, "Loading Related Data", "Understand eager loading, lazy loading, and explicit loading strategies.", "18 min"),
            new(9, "Change Tracking", "Master how EF Core tracks changes and optimize save operations.", "16 min"),
            new(10, "Transactions", "Implement database transactions for data integrity.", "16 min"),
            new(11, "Concurrency Handling", "Handle concurrent updates with optimistic concurrency tokens.", "16 min"),
            new(12, "Performance Optimization", "Optimize queries, use raw SQL, and implement caching strategies.", "21 min"),
        ],
    };

    // ───────────────────────── Advanced ─────────────────────────

    private static readonly CourseDetail DesignPatterns = new()
    {
        Slug = "design-patterns",
        Title = "Design Patterns in C#",
        Description = "Learn Gang of Four patterns, SOLID principles, and clean architecture. Write maintainable, testable, and scalable code that stands the test of time.",
        Level = CourseLevel.Advanced,
        Duration = "5 Hours",
        StudentCount = "32",
        ExerciseCount = 12,
        ProjectDescription = "Refactor a real application",
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#patterns-gradient)"/>
                <rect x="44" y="40" width="16" height="16" rx="2" fill="white"/>
                <rect x="68" y="40" width="16" height="16" rx="2" fill="white"/>
                <rect x="44" y="64" width="16" height="16" rx="2" fill="white"/>
                <rect x="68" y="64" width="16" height="16" rx="2" fill="white"/>
                <path d="M52 56v8M76 56v8M60 48h8M60 72h8" stroke="white" stroke-width="2"/>
                <defs>
                    <linearGradient id="patterns-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#ef4444"/>
                        <stop offset="1" stop-color="#dc2626"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Master all 5 SOLID principles",
            "Implement creational patterns (Factory, Builder, Singleton)",
            "Apply structural patterns (Adapter, Decorator, Facade)",
            "Use behavioral patterns (Strategy, Observer, Command)",
            "Understand when and why to use each pattern",
            "Design clean, maintainable architectures",
            "Refactor legacy code using patterns",
            "Combine patterns in real-world applications",
        ],
        Prerequisites =
        [
            "Strong understanding of C# and OOP concepts",
            "Experience building .NET applications",
            "Familiarity with interfaces and abstract classes",
            "Basic understanding of LINQ",
        ],
        Lessons =
        [
            new(1, "Introduction to Design Patterns", "Understand what design patterns are, their history, and why they matter.", "10 min"),
            new(2, "Single Responsibility Principle", "Learn the S in SOLID - one class, one responsibility.", "12 min"),
            new(3, "Open/Closed Principle", "Classes should be open for extension, closed for modification.", "12 min"),
            new(4, "Liskov Substitution Principle", "Subtypes must be substitutable for their base types.", "12 min"),
            new(5, "Interface Segregation Principle", "Many specific interfaces are better than one general interface.", "10 min"),
            new(6, "Dependency Inversion Principle", "Depend on abstractions, not concretions.", "12 min"),
            new(7, "Singleton Pattern", "Ensure a class has only one instance with global access.", "8 min"),
            new(8, "Factory Method Pattern", "Define an interface for creating objects, let subclasses decide which class to instantiate.", "12 min"),
            new(9, "Abstract Factory Pattern", "Create families of related objects without specifying concrete classes.", "12 min"),
            new(10, "Builder Pattern", "Separate construction of complex objects from their representation.", "12 min"),
            new(11, "Prototype Pattern", "Create new objects by cloning existing ones.", "8 min"),
            new(12, "Adapter Pattern", "Convert the interface of a class into another interface clients expect.", "10 min"),
            new(13, "Decorator Pattern", "Attach additional responsibilities to objects dynamically.", "12 min"),
            new(14, "Facade Pattern", "Provide a unified interface to a set of interfaces in a subsystem.", "10 min"),
            new(15, "Proxy Pattern", "Provide a surrogate or placeholder for another object.", "10 min"),
            new(16, "Composite Pattern", "Compose objects into tree structures to represent part-whole hierarchies.", "12 min"),
            new(17, "Strategy Pattern", "Define a family of algorithms and make them interchangeable.", "12 min"),
            new(18, "Observer Pattern", "Define a one-to-many dependency between objects.", "14 min"),
            new(19, "Command Pattern", "Encapsulate a request as an object.", "12 min"),
            new(20, "State Pattern", "Allow an object to alter its behavior when its internal state changes.", "12 min"),
            new(21, "Template Method Pattern", "Define the skeleton of an algorithm, deferring steps to subclasses.", "12 min"),
            new(22, "Chain of Responsibility", "Pass requests along a chain of handlers.", "12 min"),
            new(23, "Clean Architecture", "Organize code into layers with clear dependencies.", "20 min"),
            new(24, "Putting It All Together", "Apply multiple patterns in a real-world application.", "22 min"),
        ],
    };

    private static readonly CourseDetail AsyncConcurrency = new()
    {
        Slug = "async-concurrency",
        Title = "Async/Await & Concurrency",
        Description = "Deep dive into asynchronous programming, Task Parallel Library, and concurrency patterns in C#. Master async/await, parallel processing, and thread-safe code.",
        Level = CourseLevel.Advanced,
        Duration = "4 Hours",
        StudentCount = "21",
        ExerciseCount = 10,
        ProjectDescription = "Concurrent application project",
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#async-gradient)"/>
                <path d="M40 64h48" stroke="white" stroke-width="4" stroke-linecap="round"/>
                <path d="M64 40v48" stroke="white" stroke-width="4" stroke-linecap="round"/>
                <circle cx="40" cy="64" r="8" fill="white"/>
                <circle cx="88" cy="64" r="8" fill="white"/>
                <circle cx="64" cy="40" r="8" fill="white"/>
                <circle cx="64" cy="88" r="8" fill="white"/>
                <path d="M48 48l32 32M80 48l-32 32" stroke="white" stroke-width="2" stroke-dasharray="4 4"/>
                <defs>
                    <linearGradient id="async-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#f59e0b"/>
                        <stop offset="1" stop-color="#d97706"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Master async/await syntax and patterns",
            "Work with Task and ValueTask effectively",
            "Implement cancellation and progress reporting",
            "Use Parallel.For and PLINQ for data parallelism",
            "Build pipelines with TPL Dataflow",
            "Synchronize threads safely",
            "Use concurrent collections properly",
            "Debug and profile async applications",
        ],
        Prerequisites =
        [
            "Strong understanding of C# fundamentals",
            "Experience with .NET applications",
            "Basic understanding of threading concepts",
            "Familiarity with LINQ",
        ],
        Lessons =
        [
            new(1, "Introduction to Asynchronous Programming", "Understand why async matters and how it differs from parallel programming.", "12 min"),
            new(2, "Tasks and the Task-Based Asynchronous Pattern", "Learn about Task, Task<T>, and how the TAP pattern works.", "15 min"),
            new(3, "Async and Await Keywords", "Master the syntax and mechanics of async/await in C#.", "15 min"),
            new(4, "ValueTask for Performance", "Use ValueTask to reduce allocations in hot paths.", "10 min"),
            new(5, "Task Combinators", "Work with Task.WhenAll, Task.WhenAny, and custom combinators.", "15 min"),
            new(6, "Cancellation Tokens", "Implement cooperative cancellation in async operations.", "15 min"),
            new(7, "Progress Reporting", "Report progress from long-running async operations.", "10 min"),
            new(8, "Exception Handling in Async Code", "Handle exceptions properly in async methods and aggregated exceptions.", "15 min"),
            new(9, "Parallel.For and Parallel.ForEach", "Process collections in parallel with the TPL.", "15 min"),
            new(10, "PLINQ - Parallel LINQ", "Execute LINQ queries in parallel for data-intensive operations.", "15 min"),
            new(11, "Dataflow with TPL Dataflow", "Build producer-consumer pipelines with TPL Dataflow blocks.", "20 min"),
            new(12, "Thread Synchronization Primitives", "Use locks, Monitor, Mutex, and Semaphore for thread safety.", "20 min"),
            new(13, "Concurrent Collections", "Work with ConcurrentDictionary, ConcurrentQueue, and other thread-safe collections.", "15 min"),
            new(14, "Channels", "Implement producer-consumer patterns with System.Threading.Channels.", "15 min"),
            new(15, "Avoiding Common Pitfalls", "Avoid deadlocks, async void, and other common mistakes.", "10 min"),
            new(16, "Debugging and Profiling Async Code", "Use tools to diagnose issues in concurrent applications.", "18 min"),
        ],
    };

    private static readonly CourseDetail Microservices = new()
    {
        Slug = "microservices",
        Title = "Microservices with .NET",
        Description = "Build distributed systems with microservices architecture. Learn about Docker, Kubernetes, message queues, API gateways, and service-to-service communication patterns.",
        Level = CourseLevel.Advanced,
        Duration = "6 Hours",
        StudentCount = "12",
        ExerciseCount = 15,
        ProjectDescription = "Complete e-commerce microservices project",
        IconSvg = """
            <svg viewBox="0 0 128 128" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="64" cy="64" r="60" fill="url(#microservices-gradient)"/>
                <circle cx="44" cy="44" r="12" fill="white"/>
                <circle cx="84" cy="44" r="12" fill="white"/>
                <circle cx="44" cy="84" r="12" fill="white"/>
                <circle cx="84" cy="84" r="12" fill="white"/>
                <circle cx="64" cy="64" r="10" fill="white"/>
                <path d="M52 52l8 8M76 52l-8 8M52 76l8-8M76 76l-8-8" stroke="white" stroke-width="3" stroke-linecap="round"/>
                <defs>
                    <linearGradient id="microservices-gradient" x1="0" y1="0" x2="128" y2="128" gradientUnits="userSpaceOnUse">
                        <stop offset="0" stop-color="#0891b2"/>
                        <stop offset="1" stop-color="#0e7490"/>
                    </linearGradient>
                </defs>
            </svg>
            """,
        LearningOutcomes =
        [
            "Design and architect microservices systems",
            "Containerize .NET applications with Docker",
            "Orchestrate containers with Kubernetes",
            "Implement async messaging with RabbitMQ",
            "Build high-performance services with gRPC",
            "Configure API gateways and service discovery",
            "Implement distributed tracing and logging",
            "Handle failures with resilience patterns",
        ],
        Prerequisites =
        [
            "Strong experience with ASP.NET Core Web APIs",
            "Understanding of REST and HTTP fundamentals",
            "Familiarity with Entity Framework Core",
            "Basic knowledge of Docker concepts",
            "Experience with dependency injection",
            "Understanding of async/await patterns",
        ],
        Lessons =
        [
            new(1, "Introduction to Microservices", "Understand microservices architecture, benefits, challenges, and when to use it.", "12 min"),
            new(2, "Monolith to Microservices", "Learn strategies for decomposing monolithic applications.", "12 min"),
            new(3, "Domain-Driven Design Basics", "Apply DDD concepts to identify service boundaries.", "12 min"),
            new(4, "Docker Fundamentals", "Learn Docker concepts: images, containers, volumes, and networks.", "13 min"),
            new(5, "Dockerizing .NET Applications", "Create optimized Dockerfiles for .NET applications.", "13 min"),
            new(6, "Docker Compose", "Orchestrate multi-container applications locally.", "10 min"),
            new(7, "Synchronous Communication with HTTP", "Design REST APIs for service-to-service communication.", "10 min"),
            new(8, "gRPC in .NET", "Build high-performance services with Protocol Buffers and gRPC.", "13 min"),
            new(9, "Asynchronous Messaging Patterns", "Understand event-driven architecture and messaging patterns.", "12 min"),
            new(10, "RabbitMQ with MassTransit", "Implement message queues with RabbitMQ and MassTransit.", "14 min"),
            new(11, "Event Sourcing and CQRS", "Apply event sourcing and command query responsibility segregation.", "13 min"),
            new(12, "Database per Service Pattern", "Manage data in distributed systems.", "10 min"),
            new(13, "Saga Pattern", "Handle distributed transactions with sagas.", "14 min"),
            new(14, "Outbox Pattern", "Ensure reliable message publishing with the outbox pattern.", "10 min"),
            new(15, "API Gateway Pattern", "Understand the role of API gateways in microservices.", "10 min"),
            new(16, "YARP Reverse Proxy", "Build API gateways with YARP in .NET.", "12 min"),
            new(17, "Authentication and Authorization", "Implement centralized auth with IdentityServer.", "14 min"),
            new(18, "Kubernetes Fundamentals", "Learn Kubernetes concepts: pods, services, deployments.", "13 min"),
            new(19, "Deploying .NET to Kubernetes", "Create Kubernetes manifests for .NET services.", "12 min"),
            new(20, "ConfigMaps and Secrets", "Manage configuration in Kubernetes.", "10 min"),
            new(21, "Health Checks and Probes", "Implement liveness and readiness probes.", "10 min"),
            new(22, "Scaling and Load Balancing", "Configure horizontal pod autoscaling.", "12 min"),
            new(23, "Centralized Logging with Serilog", "Aggregate logs from multiple services.", "11 min"),
            new(24, "Distributed Tracing with OpenTelemetry", "Trace requests across service boundaries.", "12 min"),
            new(25, "Metrics and Monitoring", "Monitor services with Prometheus and Grafana.", "12 min"),
            new(26, "Resilience Patterns", "Implement retry, circuit breaker, and timeout patterns.", "12 min"),
            new(27, "Polly for .NET", "Use Polly for resilience and transient fault handling.", "11 min"),
            new(28, "Building a Complete Microservices System", "Build an e-commerce system with all patterns applied.", "14 min"),
        ],
    };
}
