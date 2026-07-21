using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class MethodReturnTypeDetectorTests
{
    [TestMethod]
    [DataRow("public int M() { }", MethodReturnType.Void, DisplayName = "Empty method body is detected as Void")]
    [DataRow("public int M() { return; }", MethodReturnType.Void, DisplayName = "Method with empty return statement is detected as Void")]
    [DataRow("public void M() { return 42; }", MethodReturnType.Other, DisplayName = "Void method returning value is detected as Other")]
    [DataRow("public void M() => 42;", MethodReturnType.Other, DisplayName = "Void method with expression body returning value is detected as Other")]
    [DataRow("public void M() { return System.Threading.Tasks.Task.CompletedTask; }", MethodReturnType.Task, DisplayName = "Method returning Task is detected as Task")]
    [DataRow("public void M() { return Task.CompletedTask; }", MethodReturnType.Task, DisplayName = "Method returning Task is detected as Task")]
    [DataRow("public void M() => System.Threading.Tasks.Task.CompletedTask;", MethodReturnType.Task, DisplayName = "Method with expression body returning Task is detected as Task")]
    [DataRow("public void M() { return Task.FromResult<42>; }", MethodReturnType.Other, DisplayName = "Method returning Task<int> is detected as Other")]
    [DataRow("public void M() { return Task.FromException(new AccessViolationException()); }", MethodReturnType.Task, DisplayName = "Method returning exception for Task is detected as Task")]
    [DataRow("public void M() { return Task.FromException<int>(new AccessViolationException()); }", MethodReturnType.Other, DisplayName = "Method returning exception for Task<int> is detected as Other")]
    [DataRow("public void M() { return Task.FromCanceled(CancellationToken.None); }", MethodReturnType.Task, DisplayName = "Method returning cancelled for Task is detected as Task")]
    [DataRow("public void M() { return Task.Task.FromCanceled<int>(CancellationToken.None); }", MethodReturnType.Other, DisplayName = "Method returning cancelled for Task<int> is detected as Other")]
    [DataRow("public void M() { if (true) return; return 42; return \"\"; }", MethodReturnType.Mixed, DisplayName = "Method with mixed return types is detected as Mixed")]
    [DataRow("public void M() { await DoAsync(); }", MethodReturnType.AsyncTask, DisplayName = "Method with await is detected as AsyncTask")]
    [DataRow("public void M() { await DoAsync(); await DoAsync2(); }", MethodReturnType.AsyncTask, DisplayName = "Method with multiple await statements is detected as AsyncTask")]
    [DataRow("public void M() { await foreach (var item in GetItemsAsync()) {} }", MethodReturnType.AsyncTask, DisplayName = "Method with await foreach is detected as AsyncTask")]
    [DataRow("public int M() { System.Func<int> nested = () => 42; return; }", MethodReturnType.Void, DisplayName = "Method with nested lambda returning value is detected as Void")]
    public void Detect_ReturnsExpectedResult(string code, MethodReturnType expectedReturnType)
    {
        MethodReturnTypeDetector.Detect(code).Should().Be(expectedReturnType);
    }

    public Task X()
    {
        return Task.FromCanceled<int>(CancellationToken.None);
    }

    [TestMethod]
    public void Detect_NonMethodDeclaration_ThrowsArgumentException()
    {
        var action = () => MethodReturnTypeDetector.Detect("public class C { }");

        action.Should().Throw<ArgumentException>();
    }
}