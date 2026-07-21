using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twia.StateMachine.CodeGenerator.UnitTests.Verifiers;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class StateMachineIncrementalCodeGeneratorTests
{
    private readonly IncrementalGeneratorVerifier<StateMachineIncrementalCodeGenerator> _verifier = new(
        "Twia.StateMachine.CodeGenerator.UnitTests.", 
        "_StateMachine.g.cs",
        // Suppress all diagnostics from the compiler itself. We only want to test the generator diagnostics.
        // Risk: This might hide useful information if the input sources are invalid.
        CompilerDiagnostics.None);

    public StateMachineIncrementalCodeGeneratorTests()
    {
        _verifier.AddAdditionalFileReferences("Twia.StateMachine.dll");
        _verifier.DisabledDiagnostics.Add("CS0759");
    }

    [TestMethod]
    public async Task Generator_WithNoAttribute_GeneratesNoCode()
    {
        const string code = """
            namespace Twia.StateMachine.CodeGenerator.UnitTests;
            
            public partial class UnitTestEmptyStateMachine
            {
            }
            """;

        await _verifier.VerifyGeneratorAsyncWithEmptyResult([code]);
    }

    [TestMethod]
    public async Task Generator_OnRecordType_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial record UnitTestEmptyStateMachine
                            {
                            }
                            """;

        await _verifier.VerifyGeneratorAsyncWithEmptyResult([code]);
    }

    [TestMethod]
    public async Task Generator_NotPartialClass_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;
                            
                            namespace Twia.StateMachine.CodeGenerator.UnitTests;
                            
                            [StateMachine]
                            public class UnitTestEmptyStateMachine
                            {
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0001")
            .WithLocation(6, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_NoInitialStates_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State]
                                public partial void State1()
                                {
                                }

                                [State]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0003")
            .WithLocation(6, 22)
            .WithArguments("UnitTestEmptyStateMachine");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MultipleInitialStates_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public partial void State1()
                                {
                                }

                                [State, InitialState]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0002")
            .WithLocation(14, 25)
            .WithArguments("State2", "State1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MethodIsStateAndTrigger_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [Trigger, State]
                                public partial void State1()
                                {
                                }

                                [State, InitialState]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0004")
            .WithLocation(9, 25)
            .WithArguments("State1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MethodIsNotStateButHasTransitionAttributes_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [OnEntry("DoNothing()")]
                                public partial void State1();

                                [OnExit("DoNothing()")]
                                public partial void State2();
                            
                                [Transition("Trigger1", "State1")]
                                public partial void State3();
                                
                                [TransitionAfter("PT00:00:01", "State1")]
                                public partial void State4();

                                [InitialState]
                                public partial void State5();
                                
                                [Trigger]
                                public partial void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(12, 25)
            .WithArguments("State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(15, 25)
            .WithArguments("State3");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(18, 25)
            .WithArguments("State4");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4]);
    }

    [TestMethod]
    public async Task Generator_MethodIsTriggerButHasTransitionAttributes_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [Trigger, OnEntry("DoNothing()")]
                                public partial void State1();

                                [Trigger, OnExit("DoNothing()")]
                                public partial void State2();
                            
                                [Trigger, Transition("Trigger1", "State1")]
                                public partial void State3();
                                
                                [Trigger, TransitionAfter("PT00:00:01", "State1")]
                                public partial void State4();

                                [InitialState]
                                public partial void State5();
                                
                                [Trigger]
                                public partial void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(12, 25)
            .WithArguments("State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(15, 25)
            .WithArguments("State3");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(18, 25)
            .WithArguments("State4");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4]);
    }

    [TestMethod]
    public async Task Generator_TriggerOrStateMethodNotPartial_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public void State1();

                                [Trigger]
                                public void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(9, 17)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(12, 17)
            .WithArguments("Trigger1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }
    
    [TestMethod]
    public async Task Generator_TriggerOrStateMethodNotVoid_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public partial bool State1();

                                [Trigger]
                                public partial Task Trigger1Async();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0008")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0008")
            .WithLocation(12, 25)
            .WithArguments("Trigger1Async");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_TriggerOrStateWithParameters_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;
                            using System.Threading;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public partial void State1(string name);

                                [Trigger]
                                public partial void Trigger1Async(CancellationToken ct);
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0009")
            .WithLocation(10, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0009")
            .WithLocation(13, 25)
            .WithArguments("Trigger1Async");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_WithMixOfErrors_ReportsThemAll()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public class UnitTestEmptyStateMachine
                            {
                                [Trigger, OnEntry("DoNothing()")]
                                public partial void State1();

                                [OnExit("DoNothing()")]
                                public partial void State2();
                            
                                [State]
                                public void State3();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0001")
            .WithLocation(6, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0003")
            .WithLocation(6, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(12, 25)
            .WithArguments("State2");
        var diagnostics5 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(15, 17)
            .WithArguments("State3");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4, diagnostics5]);
    }

    public TestContext TestContext { get; set; }

    [TestMethod(DisplayName = "GenerateSyncCode")]
    [DataRow("WithAttributes", DisplayName = "WithAttributes")]
    [DataRow("WithFullAttributeNames", DisplayName = "WithFullAttributeNames")]
    [DataRow("NestedClass", DisplayName = "NestedClass")]
    [DataRow("NoStateNoTriggers", DisplayName = "NoStateNoTriggers")]
    [DataRow("OnlyStatesAndNoTriggers", DisplayName = "OnlyStatesAndNoTriggers")]
    [DataRow("WithConditionsAndActions", DisplayName = "WithConditionsAndActions")]
    [DataRow("Observable", DisplayName = "Observable")]
    public async Task Generator_GeneratesSyncCode(string testDataName)
    {
        var code = await File.ReadAllTextAsync($"TestFiles/sync/{testDataName}.cs", TestContext.CancellationToken);
        var expectedCode = await File.ReadAllTextAsync($"TestFiles/sync/{testDataName}.e.cs", TestContext.CancellationToken);
#if SNAPSHOTS
        _verifier.OutputFile = Path.Join(Path.GetTempPath(), $"{testDataName}.g.cs");
#endif

        await _verifier.VerifyGeneratorAsync([code], ("*UnitTestEmptyStateMachine*", expectedCode));
    }

    [TestMethod(DisplayName = "GenerateAsyncCode")]
    [DataRow("AsyncNoStateNoTriggers", DisplayName = "Async NoStateNoTriggers")]
    [DataRow("AsyncWithAttributes", DisplayName = "Async WithAttributes")]
    [DataRow("AsyncWithEntryAndExit", DisplayName = "Async WithEntryAndExit")]
    public async Task Generator_GeneratesAsyncCode(string testDataName)
    {
        var code = await File.ReadAllTextAsync($"TestFiles/async/{testDataName}.cs", TestContext.CancellationToken);
        var expectedCode = await File.ReadAllTextAsync($"TestFiles/async/{testDataName}.e.cs", TestContext.CancellationToken);
#if SNAPSHOTS
        _verifier.OutputFile = Path.Join(Path.GetTempPath(), $"{testDataName}.g.cs");
#endif

        await _verifier.VerifyGeneratorAsync([code], ("*UnitTestEmptyStateMachine*", expectedCode));
    }
}