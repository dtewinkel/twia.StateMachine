using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine.StateMachine]
internal partial class UnitTestStateMachine
{
    private int _offCount = 0;
    private int _onCount = 0;

    [StateMachine.TriggerAttribute]
    public partial void ButtonPressed();

    [Twia.StateMachine.Transition("ButtonPressed", "On", Condition = "_offCount < 100", Action = "_offCount++")]
    [Twia.StateMachine.InitialState]
    internal partial void Off();

    [StateMachine.OnEntry("_onCount++")]
    [StateMachine.Transition("ButtonPressed", "On", Condition = "_offCount < 100", Action = "_offCount++")]
    [Twia.StateMachine.StateAttribute]
    partial void On();
}