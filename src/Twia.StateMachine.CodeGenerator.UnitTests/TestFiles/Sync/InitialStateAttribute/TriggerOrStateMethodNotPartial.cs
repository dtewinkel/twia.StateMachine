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
