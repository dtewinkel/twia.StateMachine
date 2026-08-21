using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Define a triggerless transition from the state the attribute is applied to, to the state defined in <see cref="TargetState"/>
/// when all entry and do actions are completed and the optional condition defined in <see cref="Condition"/> is met.
/// When the transition is triggered, the optional action defined in <see cref="Action"/> will be executed.
/// </summary>
/// <remarks>
/// The attribute must be applied to a method marked with the <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class TriggerlessTransitionAttribute : Attribute
{
    /// <summary>
    /// Create a new instance of the <see cref="TriggerlessTransitionAttribute"/> class.
    /// </summary>
    /// <param name="targetState">
    /// The state to transition to.
    /// This must be the name of a method marked with the <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/> in the same class as the state method this attribute is applied to.
    /// </param>
    public TriggerlessTransitionAttribute(string targetState)
    {
        TargetState = targetState;
    }

    /// <summary>
    /// The state to transition to.
    /// This must be the name of a method marked with the <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/> in the same class as the state method this attribute is applied to.
    /// </summary>
    public string TargetState { get; }

    /// <summary>
    /// The action to execute when the transition is triggered. This should be a valid C# statement or block of statements that can be executed at runtime.
    /// </summary>
    public string? Action { get; set; } = null;

    /// <summary>
    /// The condition that must be met for the transition to occur. This should be a valid C# expression that can be evaluated at runtime.
    /// The expression can reference any method or property of the state machine class, but it cannot reference any parameters of the trigger method.
    /// </summary>
    public string? Condition { get; set; } = null;
}