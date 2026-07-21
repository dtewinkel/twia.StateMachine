using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
internal partial class UnitTestEmptyStateMachine
{
    [InitialState]
    public partial void State1()
    {
    }

    [State]
    public partial void State2()
    {
    }
}