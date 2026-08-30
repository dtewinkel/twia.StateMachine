/***
* Name: Trigger Or State With Parameters
* Output: None
* Diagnostics:
* - SMG0009, 18, 25, "State1"
* - SMG0009, 21, 25, "Trigger1Async"
***/

using Twia.StateMachine;
using System.Threading;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestEmptyStateMachine
{
    [InitialState]
    public partial void State1(string name);

    [Trigger]
    public partial void Trigger1Async(CancellationToken ct);
}
