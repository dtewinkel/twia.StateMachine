using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twia.StateMachine.CodeGenerator.UnitTests.Verifiers;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class StateMachineIncrementalCodeGeneratorTests
{
    private readonly TestContext _testContext;

    private readonly IncrementalGeneratorVerifier<StateMachineIncrementalCodeGenerator> _verifier = new(typeof(StateMachineIncrementalCodeGeneratorTests));

    public StateMachineIncrementalCodeGeneratorTests(TestContext testContext)
    {
        _testContext = testContext;
        _verifier.AddAdditionalFileReferences("Twia.StateMachine.dll");
    }

    [TestMethod]
    public async Task Generator_WithNoAttribute_GeneratesNoCode()
    {
        const string code = """
            /***
            * Name: Partial class without StateMachine attribute
            * Output: None
            ***/
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
                            /***
                            * Name: StateMachine attribute On Record Type
                            * Output: None
                            ***/
                            
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
                            /***
                            * Name: StateMachine attribute not partial class
                            * Output: None
                            ***/
                            
                            using Twia.StateMachine;
                            
                            namespace Twia.StateMachine.CodeGenerator.UnitTests;
                            
                            [StateMachine]
                            public class UnitTestEmptyStateMachine
                            {
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0001")
            .WithLocation(11, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_NoInitialState_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: No Initial State
                            * Output: None
                            * Diagnostics:
                            * - SMG0003, 13, 20, "UnitTestEmptyStateMachine"
                            ***/
                            
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State]
                                public partial void State1();

                                [State]
                                public partial void State2();

                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0003")
            .WithLocation(13, 22)
            .WithArguments("UnitTestEmptyStateMachine");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MultipleInitialStates_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Multiple Initial States 
                            * Output: None
                            * Diagnostics:
                            * - SMG0002, 19, 25, "State2", "State1"
                            ***/
                            
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                public partial void State1();

                                [InitialState]
                                public partial void State2();
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0002")
            .WithLocation(19, 25)
            .WithArguments("State2", "State1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_NotExistingTriggers_GeneratesErrors()
    {
        const string code = """
                            /***
                            * Name: Not existing Triggers
                            * Output: None
                            * Diagnostics:
                            * - SMG0010, 18, 25, "Trigger1", "State1"
                            * - SMG0010, 22, 25, "Trigger2", "State2"
                            ***/
                            
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                [Transition("Trigger1", "State2")]
                                public partial void State1();

                                [State]
                                [InternalTransition("Trigger2", "Work()")]
                                public partial void State2();
                                
                                public void Work() {}
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0010")
            .WithLocation(18, 25)
            .WithArguments("Trigger1", "State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0010")
            .WithLocation(22, 25)
            .WithArguments("Trigger2", "State2");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_NotExistingStates_GeneratesErrors()
    {
        const string code = """
                            /***
                            * Name: Not existing States
                            * Output: None
                            * Diagnostics:
                            * - SMG0011, 19, 25, "State4", "State1"
                            * - SMG0011, 23, 25, "State5", "State2"
                            * - SMG0011, 27, 25, "State6", "State3"
                            ***/
                            
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                [Transition("Trigger1", "State4")]
                                public partial void State1();

                                [State]
                                [TriggerlessTransition("State5")]
                                public partial void State2();
                            
                                [State]
                                [TransitionAfter("0:00:01", "State6")]
                                public partial void State3();
                                
                                [Trigger]
                                public partial void Trigger1();
                                
                                public void Work()
                                {
                                }
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0011")
            .WithLocation(19, 25)
            .WithArguments("State4", "State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0011")
            .WithLocation(23, 25)
            .WithArguments("State5", "State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0011")
            .WithLocation(27, 25)
            .WithArguments("State6", "State3");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3]);
    }


    [TestMethod]
    public async Task Generator_InvalidPeriod_GeneratesErrors()
    {
        const string code = """
                            /***
                            * Name: Invalid Period
                            * Output: None
                            * Diagnostics:
                            * - SMG0012, 19, 25, "a", "State1"
                            * - SMG0012, 24, 25, "T1H", "State2"
                            * - SMG0012, 24, 25, "2 seconds", "State2"
                            ***/
                            
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                [TransitionAfter("a", "State2")]
                                public partial void State1();

                                [State]
                                [TransitionAfter("T1H", "State2")]
                                [TransitionAfter("2 seconds", "State2")] 
                                public partial void State2();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0012")
            .WithLocation(19, 25)
            .WithArguments("a", "State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0012")
            .WithLocation(24, 25)
            .WithArguments("T1H", "State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0012")
            .WithLocation(24, 25)
            .WithArguments("2 seconds", "State2");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3]);
    }

    [TestMethod]
    public async Task Generator_MethodIsStateAndTrigger_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Method Is State And Trigger
                            * Output: None
                            * Diagnostics:
                            * - SMG0004, 16, 25, "State1"
                            ***/

                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [Trigger, State]
                                public partial void State1()
                                {
                                }

                                [InitialState]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0004")
            .WithLocation(16, 25)
            .WithArguments("State1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MethodIsNotStateButHasTransitionAttributes_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Method Is Not State But Has Transition Attributes
                            * Output: None
                            * Diagnostics:
                            * - SMG0005, 19, 25, "State1"
                            * - SMG0005, 22, 25, "State2"
                            * - SMG0005, 25, 25, "State3"
                            * - SMG0005, 28, 25, "State4"
                            ***/

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
                                
                                [TransitionAfter("00:00:01", "State1")]
                                public partial void State4();

                                [InitialState]
                                public partial void State5();
                                
                                [Trigger]
                                public partial void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(19, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(22, 25)
            .WithArguments("State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(25, 25)
            .WithArguments("State3");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(28, 25)
            .WithArguments("State4");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4]);
    }

    [TestMethod]
    public async Task Generator_MethodIsTriggerButHasTransitionAttributes_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Method Is Trigger But Has Transition Attributes
                            * Output: None
                            * Diagnostics:
                            * - SMG0006, 19, 25, "State1"
                            * - SMG0006, 22, 25, "State2"
                            * - SMG0006, 25, 25, "State3"
                            * - SMG0006, 28, 25, "State4"
                            ***/

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
                                
                                [Trigger, TransitionAfter("00:00:01", "State1")]
                                public partial void State4();

                                [InitialState]
                                public partial void State5();
                                
                                [Trigger]
                                public partial void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(19, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(22, 25)
            .WithArguments("State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(25, 25)
            .WithArguments("State3");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(28, 25)
            .WithArguments("State4");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4]);
    }

    [TestMethod]
    public async Task Generator_TriggerOrStateMethodNotPartial_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Trigger Or State Method Not Partial
                            * Output: None
                            * Diagnostics:
                            * - SMG0007, 17, 17, "State1"
                            * - SMG0007, 20, 17, "Trigger1"
                            ***/

                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                public void State1();

                                [Trigger]
                                public void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(17, 17)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(20, 17)
            .WithArguments("Trigger1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }
    
    [TestMethod]
    public async Task Generator_TriggerOrStateMethodNotVoid_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Trigger Or State Method Not Void
                            * Output: None
                            * Diagnostics:
                            * - SMG0008, 17, 25, "State1"
                            * - SMG0008, 20, 25, "Trigger1Async"
                            ***/

                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                public partial bool State1();

                                [Trigger]
                                public partial Task Trigger1Async();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0008")
            .WithLocation(17, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0008")
            .WithLocation(20, 25)
            .WithArguments("Trigger1Async");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_TriggerOrStateWithParameters_GeneratesNoCode()
    {
        const string code = """
                            /***
                            * Name: Trigger Or State With Parameters
                            * Output: None
                            * Diagnostics:
                            * - SMG0009, 18, 25, "State1"
                            * - SMG0009, 21, 25, "Trigger1Async"
                            ***/

                            using Twia.StateMachine;
                            using System.Threading;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [InitialState]
                                public partial void State1(string name);

                                [Trigger]
                                public partial void Trigger1Async(CancellationToken ct);
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0009")
            .WithLocation(18, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0009")
            .WithLocation(21, 25)
            .WithArguments("Trigger1Async");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_WithMixOfErrors_ReportsThemAll()
    {
        const string code = """
                            /***
                            * Name: With Mix Of Errors
                            * Output: None
                            * Diagnostics:
                            * - SMG0001, 17, 14, "UnitTestEmptyStateMachine"
                            * - SMG0003, 17, 14, "UnitTestEmptyStateMachine"
                            * - SMG0006, 20, 25, "State1"
                            * - SMG0005, 23, 25, "State2"
                            * - SMG0007, 26, 17, "State3"
                            ***/

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
            .WithLocation(17, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0003")
            .WithLocation(17, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(20, 25)
            .WithArguments("State1");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(23, 25)
            .WithArguments("State2");
        var diagnostics5 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(26, 17)
            .WithArguments("State3");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4, diagnostics5]);
    }

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
        var code = await File.ReadAllTextAsync($"TestFiles/sync/{testDataName}.cs", _testContext.CancellationToken);
        var expectedCode = await File.ReadAllTextAsync($"TestFiles/sync/{testDataName}.e.cs", _testContext.CancellationToken);
#if SNAPSHOTS
        _verifier.OutputFile = Path.Join(Path.GetTempPath(), $"{testDataName}.g.cs");
#endif

        await _verifier.VerifyGeneratorAsync([code], ("*UnitTestEmptyStateMachine*", expectedCode));
    }
}