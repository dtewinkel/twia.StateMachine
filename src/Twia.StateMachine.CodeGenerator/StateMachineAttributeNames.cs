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
    public const string InternalTransitionAttributeName = "Twia.StateMachine.InternalTransitionAttribute";
    public const string InternalAfterAttributeName = "Twia.StateMachine.InternalAfterAttribute";
    public const string InternalAfterEveryAttributeName = "Twia.StateMachine.InternalAfterEveryAttribute";
    public const string OnEntryAttributeName = "Twia.StateMachine.OnEntryAttribute";
    public const string OnExitAttributeName = "Twia.StateMachine.OnExitAttribute";

    public static readonly string[] AllMethodAttributeNames = [
        TriggerAttributeName, StateAttributeName, InitialStateAttributeName,
        TransitionAttributeName, TransitionAfterAttributeName, 
        TriggerlessTransitionAttributeName, InternalTransitionAttributeName,
        InternalAfterAttributeName, InternalAfterEveryAttributeName,
        OnEntryAttributeName, OnExitAttributeName
    ];
}