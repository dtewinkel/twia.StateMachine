using System.Threading;
using System.Threading.Tasks;
using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[AsyncStateMachine]
internal partial class UnitTestEmptyAsyncStateMachine
{
    [Trigger]

    public partial Task ButtonPressed();

    [State, InitialState]
    [OnEntry("await SetOnEntryAsync(cancellationToken)")]
    [OnExit("await SetOnExitAsync(cancellationToken)")]
    [Transition(nameof(ButtonPressed), nameof(On))]
    private partial Task Off(CancellationToken cancellationToken);

    [State]
    [OnEntry("SetOnEntry()")]
    [OnExit("SetOnExit()")]
    [Transition(nameof(ButtonPressed), nameof(Off))]
    [TransitionAfter("00:00:01", nameof(Off))]
    private partial Task On(CancellationToken cancellationToken);

    private Task SetOnEntryAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private Task SetOnExitAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private void SetOnEntry()
    {
    }

    private void SetOnExit()
    {
    }
}