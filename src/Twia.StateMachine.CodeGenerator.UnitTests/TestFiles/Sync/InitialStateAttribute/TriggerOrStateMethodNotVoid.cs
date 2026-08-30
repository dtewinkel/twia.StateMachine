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
