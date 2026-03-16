---
agent: 'agent'
tools: ['changes', 'search/codebase', 'edit/editFiles', 'problems', 'search']
description: 'Get best practices for XUnit unit testing, including data-driven tests'
---

# XUnit Best Practices

Your goal is to help me write effective unit tests with XUnit, covering both standard and data-driven testing approaches.

## Project Setup

- Use a separate test project with naming convention `[ProjectName].Tests`
- Reference Microsoft.NET.Test.Sdk, xunit, and xunit.runner.visualstudio packages
- Create test classes that match the classes being tested (e.g., `CalculatorTests` for `Calculator`)
- Use .NET SDK test commands: `dotnet test` for running tests

## Test Structure

- No test class attributes required (unlike MSTest/NUnit)
- Use fact-based tests with `[Fact]` attribute for simple tests
- Follow the Arrange-Act-Assert (AAA) pattern
- Use constructor for setup and `IDisposable.Dispose()` for teardown
- Use `IClassFixture<T>` for shared context between tests in a class
- Use `ICollectionFixture<T>` for shared context between multiple test classes

## Standard Tests

- Keep tests focused on a single behavior
- Avoid testing multiple behaviors in one test method
- Use clear assertions that express intent
- Include only the assertions needed to verify the test case
- Make tests independent and idempotent (can run in any order)
- Avoid test interdependencies
- According to the book namely *[How Google Tests Software: Help me test like Google](https://books.google.de/books/about/How_Google_Tests_Software.html?id=vHlTOVTKHeUC&redir_esc=y)* written by an engineering director at Google, Google has strict rules in terms of the resource usage based on the test size. Google has defined 3 test types: Small, Medium, Large. Small tests cover small amounts of code, are usually performed as automated tests, and they correspond to unit tests in conventional testing terms. Therefore, most resources are either mocked (or stubbed), or they are not used at all in small tests. Medium tests involve two or more interacting components similar to what we know as integration tests. Tests are either executed with automation or manually (if the event is difficult or significantly expensive to automate). Resources like databases, file streams, threads are allowed to be used (fakes or mocks are generally discouraged, prefer real resources), however the systems that are in direct interaction with users are discouraged to be used medium tests. Testcontainers 💡 is a great library to create a "real" testing resources separate from the production resources so that you do not pollute production resources. Large tests cover three or more features, representing real user scenarios and use real user data sources, might take well over hours to run. These tests are measures that the software satisfies user needs, so they correspond to usability tests, or functional acceptance tests an so on. All kinds of resources are used here to make sure that the software operates as it is supposed to.

## Data-Driven Tests

- Use `[Theory]` combined with data source attributes
- Use `[InlineData]` for inline test data
- Use `[MemberData]` for method-based test data
- Use `[ClassData]` for class-based test data
- Create custom data attributes by implementing `DataAttribute`
- Use meaningful parameter names in data-driven tests

## Assertions

- Use `Assert.Equal` for value equality
- Use `Assert.Same` for reference equality
- Use `Assert.True`/`Assert.False` for boolean conditions
- Use `Assert.Contains`/`Assert.DoesNotContain` for collections
- Use `Assert.Matches`/`Assert.DoesNotMatch` for regex pattern matching
- Use `Assert.Throws<T>` or `await Assert.ThrowsAsync<T>` to test exceptions
- Use fluent assertions library for more readable assertions

## Mocking and Isolation

- Consider using NSubstitute alongside XUnit
- Mock dependencies to isolate units under test
- Use interfaces to facilitate mocking
- Consider using a DI container for complex test setups

## Test Organization

- Group tests by feature or component
- Use `[Trait("Category", "CategoryName")]` for categorization
- Use collection fixtures to group tests with shared dependencies
- Consider output helpers (`ITestOutputHelper`) for test diagnostics
- Skip tests conditionally with `Skip = "reason"` in fact/theory attributes

## References
Make use of provided web pages to generate the corresponding tests
- Fetch *[Integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0&pivots=xunit)* web page to follow the best practices to generate integration tests.
- Fetch *[MVC unit tests](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/testing?view=aspnetcore-9.0)* web page to follow the best practices to generate unit test MVC controllers.
- Fetch *[Razor Pages unit tests](https://learn.microsoft.com/en-us/aspnet/core/test/razor-pages-tests?view=aspnetcore-9.0)* web page to follow the best practices to generate unit test Razor Pages.
