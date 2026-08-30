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
