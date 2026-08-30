/***
* Name: No Initial State
* Output: None
* Diagnostics:
* - SMG0003, 13, 22, "UnitTestEmptyStateMachine"
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
