# Twia.StateMachine

A generator of simple state machines, defined by attributes and conventions, for C# applications.

## Getting started

1. Install the [Twia.StateMachine](https://www.nuget.org/packages/Twia.StateMachine) package from [NuGet](https://www.nuget.org/).
2. Create a state machine class with triggers, states and transitions. 
   - See [Overview](#overview) for a quick but insightful overview.
   - See [Attributes](#attributes) for a detailed description of the attributes that can be used to define the state machine.
3. Use the state machine.

## Overview

Generates state machines based on attributes applied to a class and methods of that class.

The following UML state machine diagram:

![Simple state machine](https://raw.githubusercontent.com/dtewinkel/Twia.StateMachine/refs/heads/main/documentation/media/generated/StateMachine.png)

Can be defined in code as:

```csharp
[StateMachine]
public partial class MyStateMachine
{
    private readonly IEngine _engine;

    public MyStateMachine(IEngine engine)
    {
        _engine = engine;
    }

    [InitialState]
    [Transition(nameof(Run), nameof(Running))]
    private partial void Stopped();

    [State]
    [OnEntry("_engine.Run()")]
    [OnExit("_engine.Stop()")]
    [Transition(nameof(Stop), nameof(Stopped))]
    [TransitionAfter("1:00:00", nameof(Stopped))]
    private partial void Running();

    [Trigger]
    public partial void Run();

    [Trigger]
    public partial void Stop();
}
```

The state machine will start in de Stopped state.

When the `Run` method is called, it will transition to the `Running` state, executing the `_engine.Run()` method on entry.

After one hour in the `Running` state, it will automatically transition back to the `Stopped` state, executing the `_engine.Stop()` method on entry if the engine is running.

When the `Stop` method is called, it will transition to the `Stopped` state, executing the `_engine.Stop()` method on entry if the engine is running.

To use the above state machine:

```csharp
var engine = new StrongEngine();
var stateMachine = new MyStateMachine(engine);
stateMachine.InitializeStateMachine(); // Initialize the state machine. Brings it to the initial state.

stateMachine.Run(); // Transitions to Running state.
stateMachine.Stop(); // Transitions to Stopped state.

...

if(stateMachine.CurrentState == MyStateMachine.State.Running)
{
    stateMachine.Stop();
}
```

## Attributes

A number of attributes are available to define the state machine.

### The state machine class

The state machine class must be marked with the `StateMachineAttribute` attribute.

The class must be declared as `partial` to allow the code generator to generate the implementation.

```csharp
[StateMachine]
public partial class MyStateMachine
{
}
```

A state machine class can be embedded in another class.

The state machine attribute has two optional parameters:

- `StateAccessible`: defautls to `true`. Set to `false` to not add the `IStateAccess<TState>` interface to the state machine class, and thus not generate the `CurrentState` property.
- `Observable`: defaults to `false`. Set to `true` to add the `IStateMachineEvents<TState>` interface to the state machine class, and thus generate the `StateChanged` event.

If both parameters are set to `false`, the state machine will not generate the `State` enum as a public type.

```csharp
[StateMachine(Observable = false, StateAccessible = false)]
public partial class MyStateMachine
{
}
```

### Triggers

Triggers may initiate a transition from one state to another.

A trigger is represented by a method. This method must be marked with the `TriggerAttribute` attribute.

To send the trigger the method must be invoked.

These methods must:

- be declared as `partial`
- have no implementation
- have a `void` return type
- have no parameters

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [Trigger]
    public partial void Run();
}
```

### States

Methods that represent states must be marked with the `StateAttribute` attribute.

These methods must:

- be declared as `partial`
- and have no implementation
- preferably be private, as they should not be called by users of the state machine

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    private partial void Running();
}
```

Exactly one of the states must be marked with the `InitialStateAttribute` attribute to indicate the starting state of the state machine. The method must follow the same rules as methods marked with the `StateAttribute`.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [InitialState]
    private partial void Stopped();
}
```

### Transitions

Transition brings the state machine from one state to another, or trigger actions internal in the state. 

#### Normal transitions

A normal transition is defined by the `Transition` attribute.

A transition is triggered by a given trigger and will bring the state machine to the target state.

A transition may have a guard condition that must evaluate to true for the transition to activate.

A transition may have an action that is executed when the transition executes.

Transitions are defined on the state the transition originates from, the source state. The target state of a transition may be another state, or it may transition back to the source state.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [Transition(nameof(Stop), nameof(Stopped))]
    private partial void Running();
}
```

The example shows that, when in state `Running`, on reception of the trigger `Stop` the state should transition to the state `Stopped`.

#### Triggerless transitions

A transition may be triggerless. In that case the `TriggerlessTransition` attribute must be used.

A transition is not triggered, but executes once the state is entered and will bring the state machine to the target state.

A transition may have a guard condition that must evaluate to true for the transition to activate.

A transition may have an action that is executed when the transition executes.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [TriggerlessTransition(nameof(Stopped))]
    [OnEntry("_engine.Stop()")]
    private partial void Stopping();
}
```

The example shows that, when in state `Stopping`, on completion of the entry action, the state should transition to the state `Stopped`.

:warning: The `Transition` and `TriggerlessTransition` attributes can only be used on methods that also have the `State` or `InitialState` attribute.

#### Guard conditions

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [Transition(nameof(Stop), nameof(Stopped), Condition = "_engine.Speed > 1000")]
    private partial void Running();
}
```

The example shows that, when in state `Running`, on reception of the trigger `Stop`, and when the condition `_engine.Speed > 1000` is true, then the state should transition to the state `Stopped`.

#### Action on transition

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [State]
    [Transition(nameof(Stop), nameof(Stopped), Action = "_engine.Stop()")]
    private partial void Running();
}
```

The example shows that, when in state `Running`, on reception of the trigger `Stop`, the action `_engine.Stop()` will be executed, and the state will transition to the state `Stopped`.

### Entry and Exit actions

Actions can be define that should be executed on entry of a state, or on exit of a state.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [InitialState]
    [Transition(nameof(Run), nameof(Running))]
    private partial void Stopped();

    [State]
    [OnEntry("_engine.Run()")]
    [OnExit("_engine.Stop()")]
    [Transition(nameof(Stop), nameof(Stopped))]
    [TransitionAfter("1:00:00", nameof(Stopped))]
    private partial void Running();
 }
```

Entry and Exit actions cal also define a guard condition.

### Internal Transitions

Internal transitions have a trigger, and an actions and can have a guard condition.

Internal after transitions have a delay, an action and can have a guard condition and a initial delay.
Internal after every transitions have an interval, an action and can have a guard condition and a initial delay.

Do activities are started once the state is entered and all `OnEntry` actions have completed. They run in a background task until they finish or the state is exited.

```csharp
[StateMachine]
public partial class MyStateMachine
{
    [InitialState]
    [Transition(nameof(Run), nameof(Running))]
    private partial void Stopped();

    [State]
    [OnEntry("_engine.Run()")]
    [OnExit("_engine.Stop()")]
    [Transition(nameof(Stop), nameof(Stopped))]
    [Internal(nameof(Run), "_engine.Run()")]
    [InternalAfter("0:00:55", "_engine.StartCooling()")]
    [InternalAfterEvery("0:00:05", "_engine.CheckTemperature()", InitialDelay = "PT1H")]
    [TransitionAfter("1:00:00", nameof(Stopped))]
    private partial void Running();
 }
```

Because internal transitions do not leave the state, their `OnExit` and `OnEntry` actions will not be executed.

## Generated methods, types, and properties

In addition to the states and trigger, the following public methods, types and properties are generated for each state machine class:

- Method `void InitializeStateMachine()`. Initializes the state machine and brings it to the initial state.
- Enum `State`, embedded in the state machine class. Contains all states of the state machine as enum members.
  Enum `State` is only generated if at least on of the `StateAccessible` and `Observable` parameters of the `StateMachineAttribute` is `true`.
- Raad-only property `State CurrentState { get; }`. Returns the current state of the state machine.
  Property `CurrentState` is only generated if the `StateAccessible` parameter of the `StateMachineAttribute` is `true`.
- Event `event EventHandler<StateChangedEventArgs<TState>>? OnStateChanged;`. Event to be notified about state changes. 
  Event `OnStateChanged` is only generated if the `Observable` parameter of the `StateMachineAttribute` is `true`.
