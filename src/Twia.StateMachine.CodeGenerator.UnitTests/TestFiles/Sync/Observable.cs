using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true, StateAccessible = false)]
internal partial class UnitTestEmptyStateMachine
{
    public bool CanTransition { get; set; } = true;

    [Trigger]
    public partial void ButtonPressed();

    [State, InitialState]
    [Transition(nameof(ButtonPressed), nameof(On), Condition = "CanTransition == true", Action = "CanTransition = false")]
    internal partial void Off();

    [State]
    partial void On();
}