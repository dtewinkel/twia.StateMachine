/***
 * Name: StateMachine with only triggers and state and no transitions
 * Output: Source
 ***/

using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
internal partial class UnitTestEmptyStateMachine
{
    [Trigger]
    public partial void ButtonPressed();

    [State, InitialState]
    internal partial void Off();

    [State]
    partial void On();
}