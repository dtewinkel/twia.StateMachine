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
