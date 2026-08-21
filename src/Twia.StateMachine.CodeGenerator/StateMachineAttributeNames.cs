namespace Twia.StateMachine.CodeGenerator;

internal static class StateMachineAttributeNames
{
    public const string StateMachineAttributeName = "Twia.StateMachine.StateMachineAttribute";
    public const string StateAttributeName = "Twia.StateMachine.StateAttribute";
    public const string TriggerAttributeName = "Twia.StateMachine.TriggerAttribute";
    public const string InitialStateAttributeName = "Twia.StateMachine.InitialStateAttribute";
    public const string TransitionAttributeName = "Twia.StateMachine.TransitionAttribute";
    public const string TransitionAfterAttributeName = "Twia.StateMachine.TransitionAfterAttribute";
    public const string TriggerlessTransitionAttributeName = "Twia.StateMachine.TriggerlessTransitionAttribute";
    public const string OnEntryAttributeName = "Twia.StateMachine.OnEntryAttribute";
    public const string OnExitAttributeName = "Twia.StateMachine.OnExitAttribute";

    public static readonly string[] AllMethodAttributeNames = [
        TriggerAttributeName, StateAttributeName, InitialStateAttributeName,
        TransitionAttributeName, TransitionAfterAttributeName, TriggerlessTransitionAttributeName,
        OnEntryAttributeName, OnExitAttributeName
    ];
}