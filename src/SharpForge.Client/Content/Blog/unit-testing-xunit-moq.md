---
title: "Unit Testing with xUnit and Moq"
category: "Testing"
date: "December 28, 2025"
readTime: "10 min read"
excerpt: "Complete guide to writing effective unit tests in C# using xUnit and Moq, including best practices and real-world examples."
tags: ["xUnit", "Moq", "Unit Testing", "C#"]
sidebar:
  - href: "#getting-started-with-xunit"
    text: "Getting Started"
  - href: "#writing-your-first-test"
    text: "First Test"
  - href: "#mocking-with-moq"
    text: "Mocking with Moq"
  - href: "#testing-async-code"
    text: "Async Testing"
  - href: "#fixtures-and-shared-context"
    text: "Fixtures"
  - href: "#best-practices"
    text: "Best Practices"
---

Unit testing is essential for maintaining code quality and catching bugs early. This guide covers xUnit, the most popular testing framework for .NET, along with Moq for mocking dependencies.

## Getting Started with xUnit

First, add the required packages to your test project:

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
```

## Writing Your First Test

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        int result = calculator.Add(2, 3);

        // Assert
        Assert.Equal(5, result);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 3, 8)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void Add_MultipleInputs_ReturnsCorrectSum(
        int a, int b, int expected)
    {
        var calculator = new Calculator();
        
        int result = calculator.Add(a, b);
        
        Assert.Equal(expected, result);
    }
}
```

## Test Organization

### Naming Conventions

Use descriptive names that explain what's being tested:

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
public void GetUser_WithValidId_ReturnsUser()
public void GetUser_WithInvalidId_ThrowsNotFoundException()
public void CreateOrder_WithEmptyCart_ThrowsValidationException()
```

### Test Class Organization

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly OrderService _sut; // System Under Test

    public OrderServiceTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _emailMock = new Mock<IEmailService>();
        _sut = new OrderService(
            _repositoryMock.Object, 
            _emailMock.Object);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_SavesAndSendsEmail()
    {
        // Arrange
        var order = new Order { Id = 1, CustomerId = 100 };
        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.CreateOrderAsync(order);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(
            r => r.SaveAsync(order), Times.Once);
        _emailMock.Verify(
            e => e.SendOrderConfirmationAsync(order), Times.Once);
    }
}
```

## Mocking with Moq

### Basic Mocking

```csharp
// Setup a mock
var userRepoMock = new Mock<IUserRepository>();

// Configure return value
userRepoMock
    .Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, Name = "John" });

// Configure for any input
userRepoMock
    .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync((int id) => new User { Id = id });

// Configure to throw
userRepoMock
    .Setup(r => r.GetByIdAsync(-1))
    .ThrowsAsync(new NotFoundException());
```

### Verifying Calls

```csharp
// Verify method was called
mock.Verify(m => m.SaveAsync(It.IsAny<User>()), Times.Once);

// Verify with specific arguments
mock.Verify(m => m.SaveAsync(
    It.Is<User>(u => u.Name == "John")), Times.Once);

// Verify never called
mock.Verify(m => m.DeleteAsync(It.IsAny<int>()), Times.Never);

// Verify call count
mock.Verify(m => m.GetAllAsync(), Times.Exactly(2));
```

### Callback and Capture

```csharp
User? capturedUser = null;

mock.Setup(r => r.SaveAsync(It.IsAny<User>()))
    .Callback<User>(u => capturedUser = u)
    .ReturnsAsync((User u) => u);

await service.CreateUserAsync(new User { Name = "Jane" });

Assert.Equal("Jane", capturedUser?.Name);
```

## Testing Async Code

```csharp
[Fact]
public async Task GetUser_ExistingUser_ReturnsUser()
{
    // Arrange
    var expectedUser = new User { Id = 1, Name = "John" };
    _repoMock
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(expectedUser);

    // Act
    var result = await _sut.GetUserAsync(1);

    // Assert
    Assert.Equal(expectedUser.Name, result.Name);
}

[Fact]
public async Task GetUser_NonExistent_ThrowsException()
{
    _repoMock
        .Setup(r => r.GetByIdAsync(999))
        .ReturnsAsync((User?)null);

    await Assert.ThrowsAsync<NotFoundException>(
        () => _sut.GetUserAsync(999));
}
```

## Testing Collections

```csharp
[Fact]
public void GetActiveUsers_ReturnsOnlyActiveUsers()
{
    var users = new List<User>
    {
        new() { Id = 1, IsActive = true },
        new() { Id = 2, IsActive = false },
        new() { Id = 3, IsActive = true }
    };

    var result = _sut.GetActiveUsers(users);

    Assert.Equal(2, result.Count());
    Assert.All(result, u => Assert.True(u.IsActive));
    Assert.Contains(result, u => u.Id == 1);
    Assert.DoesNotContain(result, u => u.Id == 2);
}
```

## Fixtures and Shared Context

```csharp
// Shared setup across tests in a class
public class DatabaseTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DatabaseTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CanQueryDatabase()
    {
        var result = _fixture.Connection.Query("SELECT 1");
        Assert.NotEmpty(result);
    }
}

public class DatabaseFixture : IDisposable
{
    public IDbConnection Connection { get; }

    public DatabaseFixture()
    {
        Connection = new SqlConnection("...");
        Connection.Open();
    }

    public void Dispose() => Connection.Dispose();
}
```

## Best Practices

- **One assertion focus** - Each test should verify one behavior
- **Arrange-Act-Assert** - Structure tests clearly
- **Don't test implementation** - Test behavior, not internal details
- **Use meaningful names** - Tests are documentation
- **Keep tests fast** - Mock external dependencies
- **Test edge cases** - Null, empty, boundary conditions

## Conclusion

Unit testing with xUnit and Moq provides a powerful foundation for ensuring code quality. Start with simple tests and gradually build a comprehensive test suite that gives you confidence in your code.
