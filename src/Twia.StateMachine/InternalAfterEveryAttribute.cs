using System.Diagnostics;

namespace Twia.StateMachine;

/// <summary>
/// Define an internal transition in the state the attribute is applied to every time when the interval specified in <see cref="Interval"/> has elapsed
/// and the optional condition defined in <see cref="Condition"/> is met.
/// When the transition is triggered, the action defined in <see cref="Action"/> will be executed.
/// </summary>
/// <remarks>
/// The attribute must be applied to a method marked with the <see cref="StateAttribute"/> or <see cref="InitialStateAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[Conditional("INCLUDE_STATE_MACHINE_ATTRIBUTES")]
public class InternalAfterEveryAttribute(
    string interval,
    string action) : Attribute
{
    /// <summary>
    /// The interval at which the transition is triggered.
    /// This should be a valid TimeSpan parseable string.
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=net-10.0#system-timespan-parse(system-string)"/> for more information on the format of the Interval string.
    /// </summary>
    public string Interval { get; private set; } = interval;

    /// <summary>
    /// The action to execute when the transition is triggered. This should be a valid C# statement or block of statements that can be executed at runtime.
    /// </summary>
    public string? Action { get; private set; } = action;

    /// <summary>
    /// The condition that must be met for the transition to occur. This should be a valid C# expression that can be evaluated at runtime.
    /// The expression can reference any method or property of the state machine class, but it cannot reference any parameters of the trigger method.
    /// </summary>
    public string? Condition { get; set; } = null;

    /// <summary>
    /// The initial delay before the transition starts occurring. This should be a valid TimeSpan parseable string.
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=net-10.0#system-timespan-parse(system-string)"/> for more information on the format of the InitialDelay string.
    /// </summary>
    public string? InitialDelay { get; set; } = null;
}