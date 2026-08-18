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
    private partial class TestStateMachine
    {
        public List<string> States { get; } = []; 
        public List<string> Transitions { get; } = [];

        public int StopCount { get; private set; }
        public int StartCount { get; private set; }
        public int StopWithDelayCount { get; private set; }
        public int AfterCount { get; private set; }
        public int StoppedEntryCount { get; private set; }
        public int StartingEntryCount { get; private set; }
        public int RunningEntryCount { get; private set; }
        public int StoppingWithDelayEntryCount { get; private set; }
        public int StoppedExitCount { get; private set; }
        public int StartingExitCount { get; private set; }
        public int RunningExitCount { get; private set; }
        public int StoppingWithDelayExitCount { get; private set; }

        public bool CanStart { get; set; } = true;

        [State]
        [Transition(nameof(Stop), nameof(Stopped), Action = "StopCount++; Transitions.Add(\"StopAction\")")]
        [Transition(nameof(StopWithDelay), nameof(StoppingWithDelay), Action = "StopWithDelayCount++; Transitions.Add(\"StopWithDelayAction\")")]
        [OnEntry("RunningEntryCount++; States.Add(\"Running\"); Transitions.Add(\"RunningEntry\")")]
        [OnExit("RunningExitCount++; Transitions.Add(\"RunningExit\")")]
        private partial void Running();

        [State]
        [TransitionAfter("0:00:00.200", nameof(Stopped), Action = "AfterCount++; Transitions.Add(\"AfterAction\")")]
        [OnEntry("StoppingWithDelayEntryCount++; States.Add(\"StoppingWithDelay\"); Transitions.Add(\"StoppingWithDelayEntry\")")]
        [OnExit("StoppingWithDelayExitCount++; Transitions.Add(\"StoppingWithDelayExit\")")]
        private partial void StoppingWithDelay();

        [InitialState]
        [Transition(nameof(Start), nameof(Running), Action = $"{nameof(StartOnStoppedAction)}()", Condition = $"{nameof(StartOnStoppedCondition)}")]
        [OnEntry($"{nameof(OnEntryStoppedAction)}()")]
        [OnExit($"{nameof(OnExitStoppedAction)}()")]
        private partial void Stopped();

        [Trigger]
        public partial void Start();

        [Trigger]
        public partial void Stop();

        [Trigger]
        public partial void StopWithDelay();

        public void OnEntryStoppedAction()
        {
            StoppedEntryCount++;
            States.Add("Stopped");
            Transitions.Add("StoppedEntry");
        }

        public void OnExitStoppedAction()
        {
            StoppedExitCount++;
            Transitions.Add("StoppedExit");
        }

        public void StartOnStoppedAction()
        {
            StartCount++;
            Transitions.Add("StartAction");
        }

        public bool StartOnStoppedCondition
        {
            get
            {
                Transitions.Add($"Condition StartOnStopped={CanStart}");
                return CanStart;
            }
        }

        public void OnEntryStartingAction()
        {
            StartingEntryCount++;
            States.Add("Starting");
            Transitions.Add("StartingEntry");
        }

        public void OnExitStartingAction()
        {
            StartingExitCount++;
            Transitions.Add("StartingExit");
        }

        public void TriggerlessOnStartingAction()
        {
            StartCount++;
            Transitions.Add("StartingAction");
        }

        public bool TriggerlessOnStartingCondition
        {
            get
            {
                Transitions.Add($"Condition TriggerlessOnStarting={CanStart}");
                return CanStart;
            }
        }
    }

    [TestMethod]
    public void InitializeStateMachine_SetsInitialStateToStopped()
    {
        var stateMachine = new TestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.StartCount.Should().Be(0);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(0);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(0);
        stateMachine.States.Should().BeEquivalentTo("Stopped");
        stateMachine.Transitions.Should().BeEquivalentTo("StoppedEntry");
    }

    [TestMethod]
    public void Start_InStoppedState_TransitionsToRunningState()
    {
        var stateMachine = new TestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);

        stateMachine.Start();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Running);
        stateMachine.StartCount.Should().Be(1);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(1);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(1);
        stateMachine.States.Should().BeEquivalentTo(["Stopped", "Running"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo(["StoppedEntry", "Condition StartOnStopped=True", "StoppedExit", "StartAction", "RunningEntry"]
            , options => options.WithStrictOrdering());
    }

    [TestMethod]
    public void Start_InStoppedState_WithFailingCondition_StaysInStoppedState()
    {
        var stateMachine = new TestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.CanStart = false;

        stateMachine.Start();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.StartCount.Should().Be(0);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(0);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(0);
        stateMachine.States.Should().BeEquivalentTo(["Stopped"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo(["StoppedEntry", "Condition StartOnStopped=False"]
            , options => options.WithStrictOrdering());
    }

    [TestMethod]
    public void Stop_InRunningState_TransitionsToStoppedState()
    {
        var stateMachine = new TestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);

        stateMachine.Start();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Running);
        stateMachine.StartCount.Should().Be(1);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(1);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(1);
        stateMachine.States.Should().BeEquivalentTo(["Stopped", "Running"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo([
                "StoppedEntry", "Condition StartOnStopped=True", "StoppedExit", "StartAction", "RunningEntry"],
            options => options.WithStrictOrdering());

        stateMachine.Stop();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.StartCount.Should().Be(1);
        stateMachine.StopCount.Should().Be(1);
        stateMachine.RunningEntryCount.Should().Be(1);
        stateMachine.StoppedEntryCount.Should().Be(2);
        stateMachine.RunningExitCount.Should().Be(1);
        stateMachine.StoppedExitCount.Should().Be(1);
        stateMachine.States.Should().BeEquivalentTo(["Stopped", "Running", "Stopped"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo([
            "StoppedEntry", "Condition StartOnStopped=True", "StoppedExit", "StartAction", "RunningEntry",
            "RunningExit", "StopAction", "StoppedEntry"],
            options => options.WithStrictOrdering());
    }


     [TestMethod]
    public void StopWithDelay_InRunningState_TransitionsToStoppingAndAfterDelayToStoppedState()
    {
        var stateMachine = new TestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);

        stateMachine.Start();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Running);
        stateMachine.StartCount.Should().Be(1);
        stateMachine.StopWithDelayCount.Should().Be(0);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.AfterCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(1);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(1);
        stateMachine.States.Should().BeEquivalentTo(["Stopped", "Running"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo([
                "StoppedEntry", "Condition StartOnStopped=True", "StoppedExit", "StartAction", "RunningEntry"],
            options => options.WithStrictOrdering());

        stateMachine.StopWithDelay();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.StoppingWithDelay);
        stateMachine.StartCount.Should().Be(1);
        stateMachine.StopWithDelayCount.Should().Be(1);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.AfterCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(1);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.StoppingWithDelayEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(1);
        stateMachine.StoppedExitCount.Should().Be(1);
        stateMachine.StoppingWithDelayExitCount.Should().Be(0);
        stateMachine.States.Should().BeEquivalentTo(["Stopped", "Running", "StoppingWithDelay"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo([
                "StoppedEntry", "Condition StartOnStopped=True", "StoppedExit", "StartAction", "RunningEntry",
                "RunningExit", "StopWithDelayAction", "StoppingWithDelayEntry"],
            options => options.WithStrictOrdering());

        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        stateMachine.CurrentState.Should().Be(TestStateMachine.State.StoppingWithDelay);

        Thread.Sleep(TimeSpan.FromMilliseconds(150));
        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.StartCount.Should().Be(1);
        stateMachine.StopWithDelayCount.Should().Be(1);
        stateMachine.StopCount.Should().Be(0);
        stateMachine.AfterCount.Should().Be(1);
        stateMachine.RunningEntryCount.Should().Be(1);
        stateMachine.StoppedEntryCount.Should().Be(2);
        stateMachine.StoppingWithDelayEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(1);
        stateMachine.StoppedExitCount.Should().Be(1);
        stateMachine.StoppingWithDelayExitCount.Should().Be(1);
        stateMachine.States.Should().BeEquivalentTo(["Stopped", "Running", "StoppingWithDelay", "Stopped"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo([
                "StoppedEntry", "Condition StartOnStopped=True", "StoppedExit", "StartAction", "RunningEntry",
                "RunningExit", "StopWithDelayAction", "StoppingWithDelayEntry", "StoppingWithDelayExit", "AfterAction", "StoppedEntry"],
            options => options.WithStrictOrdering());
    }

    [TestMethod]
    public void Stop_InStoppedState_IgnoresTrigger()
    {
        var stateMachine = new TestStateMachine();
        stateMachine.InitializeStateMachine();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.StartCount.Should().Be(0);
        // Stop action is not executed because we are already in Stopped state, so StopCount remains 0.
        stateMachine.StopCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(0);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(0);
        stateMachine.States.Should().BeEquivalentTo(["Stopped"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo(["StoppedEntry"], options => options.WithStrictOrdering());

        stateMachine.Stop();

        stateMachine.CurrentState.Should().Be(TestStateMachine.State.Stopped);
        stateMachine.StartCount.Should().Be(0);
        // Stop action is not executed because we are already in Stopped state, so StopCount remains 0.
        stateMachine.StopCount.Should().Be(0);
        stateMachine.RunningEntryCount.Should().Be(0);
        stateMachine.StoppedEntryCount.Should().Be(1);
        stateMachine.RunningExitCount.Should().Be(0);
        stateMachine.StoppedExitCount.Should().Be(0);
        stateMachine.States.Should().BeEquivalentTo(["Stopped"], options => options.WithStrictOrdering());
        stateMachine.Transitions.Should().BeEquivalentTo(["StoppedEntry"], options => options.WithStrictOrdering());
    }
}
