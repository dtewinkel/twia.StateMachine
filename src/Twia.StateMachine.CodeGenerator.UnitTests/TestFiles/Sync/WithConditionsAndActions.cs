/***
 * Name: StateMachine with conditions and actions on transitions
 * Output: Source
 ***/

using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
internal partial class UnitTestEmptyStateMachine
{
    public bool CanTransition { get; set; } = true;
    public bool CanTransitionToOn { get; set; } = true;
    public bool CanTransitionTriggerless { get; set; } = false;
    public bool CanTransitionInternal { get; set; } = true;

    [Trigger]
    public partial void ButtonPressed();

    [Trigger]
    public partial void SensorOn();

    [State, InitialState]
    [Transition(nameof(ButtonPressed), nameof(On), Condition = "CanTransition == true", Action = "CanTransition = false")]
    [Transition(nameof(SensorOn), nameof(On), Condition = "CanTransitionToOn == true", Action = "CanTransitionToOn = false")]
    [InternalTransition(nameof(SensorOn), "CanTransitionInternal = false", Condition = "CanTransitionInternal == true")]
    [TriggerlessTransition(nameof(On), Condition = "CanTransitionTriggerless == true", Action = "CanTransitionTriggerless = false")]
    internal partial void Off();

    [State]
    partial void On();
}