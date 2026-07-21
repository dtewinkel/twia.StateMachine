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
    internal partial Task Off(CancellationToken ct);

    [State]
    partial Task On();
}