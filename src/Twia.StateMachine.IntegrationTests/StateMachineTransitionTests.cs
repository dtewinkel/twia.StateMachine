using System;
using System.Collections.Generic;
using System.Threading;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twia.StateMachine.IntegrationTests;

[TestClass]
public partial class StateMachineTransitionTests
{
    [StateMachine]
    private partial class UnitTestStateMachine
    {
        public int[] OnEntryCounts { get; } = [0, 0, 0];
        public int[] OnExitCounts { get; } = [0, 0, 0];
        public int[] TriggerCounts { get; } = [0, 0, 0, 0, 0];
        public List<string> Transitions { get; } = [];

        public bool Condition0 { get; set; } = true;

        public bool Condition1 { get; set; } = true;

        public bool Condition2 { get; set; } = false;

        [InitialState]
        [Transition(nameof(Trigger0), nameof(State0), Action = "SetTrigger(State.State0, State.State0, 0)", Condition = nameof(Condition0))]
        [Transition(nameof(Trigger1), nameof(State1), Action = "SetTrigger(State.State0, State.State1, 1)", Condition = nameof(Condition0))]
        [Transition(nameof(Trigger2), nameof(State2), Action = "SetTrigger(State.State0, State.State2, 2)", Condition = nameof(Condition0))]
        [TransitionAfter("0:00:00.200", nameof(State2), Action = "SetTrigger(State.State0, State.State2, 3)", Condition = nameof(Condition0))]
        [OnEntry("SetEntry(0)")]
        [OnExit("SetExit(0)")]
        private partial void State0();

        [State]
        [Transition(nameof(Trigger0), nameof(State0), Action = "SetTrigger(State.State1, State.State0, 0)", Condition = nameof(Condition0))]
        [TriggerlessTransition(nameof(State2), Action = "SetTrigger(State.State1, State.State2, 4)", Condition = nameof(Condition1))]
        [OnEntry("SetEntry(1)")]
        [OnExit("SetExit(1)")]
        private partial void State1();

        [State]
        [Transition(nameof(Trigger1), nameof(State1), Action = "SetTrigger(State.State2, State.State0, 0)", Condition = nameof(Condition0))]
        [TriggerlessTransition(nameof(State1), Action = "SetTrigger(State.State2, State.State1, 4)", Condition = nameof(Condition2))]
        [OnEntry("SetEntry(2)")]
        [OnExit("SetExit(2)")]
        private partial void State2();

        [Trigger]
        public partial void Trigger0();

        [Trigger]
        public partial void Trigger1();

        [Trigger]
        public partial void Trigger2();

        private void SetEntry(int stateIndex)
        {
            OnEntryCounts[stateIndex]++;
        }

        private void SetExit(int stateIndex)
        {
            OnExitCounts[stateIndex]++;
        }

        private void SetTrigger(State fromState, State toState, int triggerIndex)
        {
            TriggerCounts[triggerIndex]++;
            Transitions.Add($"{triggerIndex}: {fromState} -> {toState}");
        }
    }

    [TestMethod]
    public void InitializeStateMachine_SetsInitialState()
    {
        var stateMachine = new UnitTestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();
    }

    [TestMethod]
    public void Trigger0_SetsStateTo0()
    {
        var stateMachine = new UnitTestStateMachine()
        {
            Condition1 = false
        };

        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        stateMachine.Trigger0();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(2, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(1, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal("0: State0 -> State0");
    }

    [TestMethod]
    public void Trigger1_AndFailingCondition_SetsStateTo1()
    {
        var stateMachine = new UnitTestStateMachine
        {
            Condition1 = false
        };
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0 , 0);
        stateMachine.Transitions.Should().Equal();

        stateMachine.Trigger1();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State1);
        stateMachine.OnEntryCounts.Should().Equal(1, 1, 0);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 1, 0, 0, 0);
        stateMachine.Transitions.Should().Equal("1: State0 -> State1");
    }


    [TestMethod]
    public void Trigger1_AndFailingCondition_WhenConditionChanges_DoesNotTransition()
    {
        var stateMachine = new UnitTestStateMachine
        {
            Condition1 = false
        };
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        stateMachine.Trigger1();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State1);
        stateMachine.OnEntryCounts.Should().Equal(1, 1, 0);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 1, 0, 0, 0);
        stateMachine.Transitions.Should().Equal("1: State0 -> State1");

        stateMachine.Condition1 = true;

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State1);
        stateMachine.OnEntryCounts.Should().Equal(1, 1, 0);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 1, 0, 0, 0);
        stateMachine.Transitions.Should().Equal("1: State0 -> State1");

        stateMachine.Trigger1(); // Ignored.

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State1);
        stateMachine.OnEntryCounts.Should().Equal(1, 1, 0);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 1, 0, 0, 0);
        stateMachine.Transitions.Should().Equal("1: State0 -> State1");

        stateMachine.Trigger0();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(2, 1, 0);
        stateMachine.OnExitCounts.Should().Equal(1, 1, 0);
        stateMachine.TriggerCounts.Should().Equal(1, 1, 0, 0, 0);
        stateMachine.Transitions.Should().Equal("1: State0 -> State1", "0: State1 -> State0");
    }

    [TestMethod]
    public void Trigger1_AndSuccessfulCondition_FallsThroughFromState1ToState2()
    {
        var stateMachine = new UnitTestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        stateMachine.Condition1 = true;

        stateMachine.Trigger1();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State2);
        stateMachine.OnEntryCounts.Should().Equal(1, 1, 1);
        stateMachine.OnExitCounts.Should().Equal(1, 1, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 1, 0, 0, 1);
        stateMachine.Transitions.Should().Equal("1: State0 -> State1", "4: State1 -> State2");
    }

    [TestMethod]
    public void Trigger2_SetsStateTo2()
    {
        var stateMachine = new UnitTestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        stateMachine.Trigger2();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State2);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 1);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 1, 0, 0);
        stateMachine.Transitions.Should().Equal("2: State0 -> State2");
    }

    [TestMethod]
    public void Trigger2_WithFailingCondition_DoesNotTransition()
    {
        var stateMachine = new UnitTestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        stateMachine.Condition0 = false;
        stateMachine.Trigger2();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();
    }

    [TestMethod]
    public void AfterTrigger_SetsStateTo2()
    {
        var stateMachine = new UnitTestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        Thread.Sleep(TimeSpan.FromMilliseconds(100));

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        Thread.Sleep(TimeSpan.FromMilliseconds(250));

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State2);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 1);
        stateMachine.OnExitCounts.Should().Equal(1, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 1, 0);
        stateMachine.Transitions.Should().Equal("3: State0 -> State2");
    }

    [TestMethod]
    public void AfterTrigger_WithFailingCondition_DoesNotTransition()
    {
        var stateMachine = new UnitTestStateMachine
        {
            Condition0 = false
        };
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        Thread.Sleep(TimeSpan.FromMilliseconds(100));

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();

        Thread.Sleep(TimeSpan.FromMilliseconds(250));

        stateMachine.CurrentState.Should().Be(UnitTestStateMachine.State.State0);
        stateMachine.OnEntryCounts.Should().Equal(1, 0, 0);
        stateMachine.OnExitCounts.Should().Equal(0, 0, 0);
        stateMachine.TriggerCounts.Should().Equal(0, 0, 0, 0, 0);
        stateMachine.Transitions.Should().Equal();
    }
}
