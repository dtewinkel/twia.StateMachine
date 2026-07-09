using Twia.StateMachine.CodeGenerator.Builders.Async;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twia.StateMachine.CodeGenerator.UnitTests.Builders.Async;

[TestClass]
public class StatesManagementBuilderTests
{
    [TestMethod]
    [Description("IsAsync should return true when method contains a single await expression")]
    public void IsAsync_MethodWithSingleAwait_ReturnsTrue()
    {
        const string code = """
                            public async Task MyMethodAsync()
                            {
                                await Task.Delay(100);
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(true);
    }

    [TestMethod]
    [Description("IsAsync should return true when method contains multiple await expressions")]
    public void IsAsync_MethodWithMultipleAwaits_ReturnsTrue()
    {
        const string code = """
                            public async Task MyMethodAsync()
                            {
                                await Task.Delay(100);
                                var result = await GetDataAsync();
                                await Task.Delay(50);
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(true);
    }

    [TestMethod]
    [Description("IsAsync should return false when method does not contain await expression")]
    public void IsAsync_MethodWithoutAwait_ReturnsFalse()
    {
        const string code = """
                            public void MyMethod()
                            {
                                Thread.Sleep(100);
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return false when method is empty")]
    public void IsAsync_EmptyMethod_ReturnsFalse()
    {
        const string code = """
                            public void MyMethod()
                            {
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return false when passed invalid code (non-method)")]
    public void IsAsync_InvalidCode_ReturnsFalse()
    {
        const string code = "int x = 42;";

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return false when passed class declaration")]
    public void IsAsync_ClassDeclaration_ReturnsFalse()
    {
        const string code = """
                            public class MyClass
                            {
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return true when method contains await in nested block")]
    public void IsAsync_MethodWithAwaitInNestedBlock_ReturnsTrue()
    {
        const string code = """
                            public async Task MyMethodAsync()
                            {
                                if (true)
                                {
                                    await Task.Delay(100);
                                }
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(true);
    }

    [TestMethod]
    [Description("IsAsync should return true when method contains await in try-catch")]
    public void IsAsync_MethodWithAwaitInTryCatch_ReturnsTrue()
    {
        const string code = """
                            public Task MyMethodAsync()
                            {
                                try
                                {
                                    await Task.Delay(100);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(true);
    }

    [TestMethod]
    [Description("IsAsync should return true when method contains await in foreach")]
    public void IsAsync_MethodWithAwaitInForeach_ReturnsTrue()
    {
        const string code = """
                            public Task MyMethodAsync()
                            {
                                await foreach (var item in GetItemsAsync())
                                {
                                }
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(true);
    }

    [TestMethod]
    [Description("IsAsync should return true when method contains await in foreach")]
    public void IsAsync_MethodWithoutAwaitInForeach_ReturnsFalse()
    {
        const string code = """
                            public Task MyMethodAsync()
                            {
                                foreach (var item in GetItems())
                                {
                                }
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return false when method has only comments")]
    public void IsAsync_MethodWithOnlyComments_ReturnsFalse()
    {
        const string code = """
                            public void MyMethod()
                            {
                                // This is a comment
                                // Another comment
                            }
                            """;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return false when code is empty string")]
    public void IsAsync_EmptyString_ReturnsFalse()
    {
        var code = string.Empty;

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }

    [TestMethod]
    [Description("IsAsync should return false when code contains only whitespace")]
    public void IsAsync_OnlyWhitespace_ReturnsFalse()
    {
        const string code = "   \n   \t   ";

        var result = StatesManagementBuilder.IsAsync(code);

        result.Should().Be(false);
    }
}

