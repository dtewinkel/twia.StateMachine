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
