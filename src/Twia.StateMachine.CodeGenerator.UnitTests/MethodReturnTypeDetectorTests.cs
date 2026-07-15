using AwesomeAssertions;
using Twia.StateMachine.CodeGenerator;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class MethodReturnTypeDetectorTests
{
    [TestMethod]
    public void Detect_ReturnsExpectedResult()
    {
        var testCases = new (string Code, MethodReturnType ExpectedReturnType)[]
        {
            ("public int M() { }", MethodReturnType.ReturnsVoid),
            ("public int M() { return; }", MethodReturnType.ReturnsVoid),
            ("public void M() { return 42; }", MethodReturnType.ReturnOther),
            ("public void M() => 42;", MethodReturnType.ReturnOther),
            ("public void M() { return System.Threading.Tasks.Task.CompletedTask; }", MethodReturnType.ReturnTask),
            ("public void M() => System.Threading.Tasks.Task.CompletedTask;", MethodReturnType.ReturnTask),
            ("public void M() { if (true) return; return 42; }", MethodReturnType.MixedReturn),
            ("public void M() { await DoAsync(); }", MethodReturnType.ReturnTask),
            ("public int M() { System.Func<int> nested = () => 42; return; }", MethodReturnType.ReturnsVoid)
        };

        foreach (var (code, expectedReturnType) in testCases)
        {
            MethodReturnTypeDetector.Detect(code).Should().Be(expectedReturnType);
        }
    }

    [TestMethod]
    public void Detect_NonMethodDeclaration_ThrowsArgumentException()
    {
        var action = () => MethodReturnTypeDetector.Detect("public class C { }");

        action.Should().Throw<ArgumentException>();
    }
}
