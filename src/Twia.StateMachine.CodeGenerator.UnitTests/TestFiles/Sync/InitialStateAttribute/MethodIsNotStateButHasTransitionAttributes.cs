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
