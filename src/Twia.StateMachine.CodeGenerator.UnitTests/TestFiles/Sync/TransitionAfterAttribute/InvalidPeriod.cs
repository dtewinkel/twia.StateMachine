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
