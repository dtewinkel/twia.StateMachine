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
