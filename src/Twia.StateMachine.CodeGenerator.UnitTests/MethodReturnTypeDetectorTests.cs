using AwesomeAssertions;
using Twia.StateMachine.CodeGenerator;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class MethodReturnTypeDetectorTests
{
    [DataTestMethod]
    [DataRow("public int M() { }", MethodReturnType.ReturnsVoid)]
    [DataRow("public int M() { return; }", MethodReturnType.ReturnsVoid)]
    [DataRow("public void M() { return 42; }", MethodReturnType.ReturnOther)]
    [DataRow("public void M() => 42;", MethodReturnType.ReturnOther)]
    [DataRow("public void M() { return System.Threading.Tasks.Task.CompletedTask; }", MethodReturnType.ReturnTask)]
    [DataRow("public void M() => System.Threading.Tasks.Task.CompletedTask;", MethodReturnType.ReturnTask)]
    [DataRow("public void M() { if (true) return; return 42; }", MethodReturnType.MixedReturn)]
    [DataRow("public void M() { await DoAsync(); }", MethodReturnType.ReturnTask)]
    [DataRow("public int M() { System.Func<int> nested = () => 42; return; }", MethodReturnType.ReturnsVoid)]
    public void Detect_ReturnsExpectedResult(string code, MethodReturnType expectedReturnType)
    {
        MethodReturnTypeDetector.Detect(code).Should().Be(expectedReturnType);
    }

    [TestMethod]
    public void Detect_NonMethodDeclaration_ThrowsArgumentException()
    {
        var action = () => MethodReturnTypeDetector.Detect("public class C { }");

        action.Should().Throw<ArgumentException>();
    }
}
