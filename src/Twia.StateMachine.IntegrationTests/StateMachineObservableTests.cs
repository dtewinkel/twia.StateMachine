using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Twia.StateMachine.IntegrationTests;

[TestClass]
public partial class StateMachineObservableTests
{
    [StateMachine(Observable = true)]
    private partial class TestStateMachine
    {
        public bool Allowed { get; set; }

        [InitialState]
        [Transition(nameof(Trigger1), nameof(State1))]
        [Transition(nameof(Trigger2), nameof(State2))]
        [Transition(nameof(Trigger3), nameof(State3), Condition = "Allowed == true")]
        private partial void State1();

        [State]
        [Transition(nameof(Trigger3), nameof(State1))]
        [Transition(nameof(Trigger1), nameof(State3))]
        [TransitionAfter("0:00:00.500", nameof(State2))]
        private partial void State2();

        [State]
        [Transition(nameof(Trigger1), nameof(State1))]
        private partial void State3();

        [Trigger]
        public partial void Trigger1();

        [Trigger]
        public partial void Trigger2();

        [Trigger]
        public partial void Trigger3();
    }

    private readonly List<StateChangedEventArgs<TestStateMachine.State>> _events = [];

    void StateMachineOnStateChanged(object? sender, StateChangedEventArgs<TestStateMachine.State> e)
    {
        _events.Add(e);
    }

    [TestMethod]
    public void InitializeStateMachine_ReportsInitialState()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State1);

        var eventArgs = _events.Single();

        eventArgs.FromState.Should().BeNull();
        eventArgs.ToState.Should().Be(TestStateMachine.State.State1);
        eventArgs.Reason.Should().Be("Initial");
    }

    [TestMethod]
    public void InitializeStateMachine_ReportsStateChangeToSameState()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Trigger1();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State1);

        _events.Count.Should().Be(2);
        var eventArgs = _events.Last();

        eventArgs.FromState.Should().Be(TestStateMachine.State.State1);
        eventArgs.ToState.Should().Be(TestStateMachine.State.State1);
        eventArgs.Reason.Should().Be("Trigger: Trigger1");
    }

    [TestMethod]
    public void InitializeStateMachine_ReportsStateChangeToOtherState()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Trigger2();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State2);

        _events.Count.Should().Be(2);
        var eventArgs = _events.Last();

        eventArgs.FromState.Should().Be(TestStateMachine.State.State1);
        eventArgs.ToState.Should().Be(TestStateMachine.State.State2);
        eventArgs.Reason.Should().Be("Trigger: Trigger2");
    }

    [TestMethod]
    public void InitializeStateMachine_ReportsStateChangeToNextState()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Trigger2();
        stateMachine.Trigger1();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State3);

        _events.Count.Should().Be(3);
        var eventArgs = _events.Last();

        eventArgs.FromState.Should().Be(TestStateMachine.State.State2);
        eventArgs.ToState.Should().Be(TestStateMachine.State.State3);
        eventArgs.Reason.Should().Be("Trigger: Trigger1");
    }

    [TestMethod]
    public void InitializeStateMachine_ReportsAfterStateChange()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Trigger2();

        Thread.Sleep(TimeSpan.FromMilliseconds(750));

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State2);

        _events.Count.Should().Be(3);

        var eventArgs = _events.Last();
        eventArgs.FromState.Should().Be(TestStateMachine.State.State2);
        eventArgs.ToState.Should().Be(TestStateMachine.State.State2);
        eventArgs.Reason.Should().Be("After: 0:00:00.500");
    }

    [TestMethod]
    public void InitializeStateMachine_WhenTransitionIsBlocked_ReportsNoStateChange()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Allowed = false;
        stateMachine.Trigger3();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State1);

        _events.Count.Should().Be(1);
    }

    [TestMethod]
    public void InitializeStateMachine_WhenTransitionIsAllowed_ReportsStateChange()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Allowed = true;
        stateMachine.Trigger3();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State3);

        _events.Count.Should().Be(2);

        var eventArgs = _events.Last();
        eventArgs.FromState.Should().Be(TestStateMachine.State.State1);
        eventArgs.ToState.Should().Be(TestStateMachine.State.State3);
        eventArgs.Reason.Should().Be("Trigger: Trigger3");
    }


    [TestMethod]
    public void InitializeStateMachine_ReportsStateChange()
    {
        var stateMachine = new TestStateMachine();

        stateMachine.OnStateChanged += StateMachineOnStateChanged;
        stateMachine.InitializeStateMachine();

        stateMachine.Allowed = true;
        stateMachine.Trigger3();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.State3);

        _events.Count.Should().Be(2);

        var eventArgs = _events.Last();
        eventArgs.FromState.Should().Be(TestStateMachine.State.State1);
        eventArgs.ToState.Should().Be(TestStateMachine.State.State3);
        eventArgs.Reason.Should().Be("Trigger: Trigger3");
    }
}