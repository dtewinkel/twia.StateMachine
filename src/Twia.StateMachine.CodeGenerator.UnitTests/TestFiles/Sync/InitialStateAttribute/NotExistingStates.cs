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
