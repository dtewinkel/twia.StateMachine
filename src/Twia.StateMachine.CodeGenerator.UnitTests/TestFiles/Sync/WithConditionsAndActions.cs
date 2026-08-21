using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
internal partial class UnitTestEmptyStateMachine
{
    public bool CanTransition { get; set; } = true;
    public bool CanTransitionTriggerless { get; set; } = false;

    [Trigger]
    public partial void ButtonPressed();

    [State, InitialState]
    [Transition(nameof(ButtonPressed), nameof(On), Condition = "CanTransition == true", Action = "CanTransition = false")]
    [TriggerlessTransition(nameof(On), Condition = "CanTransitionTriggerless == true", Action = "CanTransitionTriggerless = false")]
    internal partial void Off();

    [State]
    partial void On();
}