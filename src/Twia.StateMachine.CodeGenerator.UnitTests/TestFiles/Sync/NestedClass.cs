using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

public partial class GrandParentClass
{
    public partial class ParentClass
    {
        [StateMachine]
        public partial class UnitTestStateMachine
        {
            [TriggerAttribute]
            public partial void ButtonPressed();

            [Transition("ButtonPressed", "Off")]
            [InitialState]
            private partial void Off();
        }
    }
}