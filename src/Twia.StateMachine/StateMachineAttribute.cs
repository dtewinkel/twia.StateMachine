using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Indicate that the class is a state machine and should have state machine logic generated.
/// </summary>
/// <remarks>
/// The target type must be a class and be marked as partial. 
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class StateMachineAttribute : Attribute
{
    /// <summary>
    /// Add accessibility to the current state of the state machine. This allows external code to read the current state, but not change it.
    /// Default is true, which means the current state is accessible. Setting this to false will make the current state inaccessible.
    /// </summary>
    public bool StateAccessible { get; set; } = true;

    /// <summary>
    /// Create an OnStateChanged event that can be observed by external code.
    /// </summary>
    public bool Observable { get; set; } = false;
}