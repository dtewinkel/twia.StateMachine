namespace Twia.StateMachine.Example;

[StateMachine(Observable = true, StateAccessible = true)]
public partial class LightingStateMachine
{
    private readonly ILightSwitch _lightSwitch;
    private readonly ILightSensor _lightSensor;

    public LightingStateMachine(ILightSwitch lightSwitch, ILightSensor lightSensor)
    {
        _lightSwitch = lightSwitch;
        _lightSensor = lightSensor;
    }

    [OnEntry("_lightSwitch.ToOff()")]
    [Transition(nameof(PresenceDetected), nameof(LightOn), Condition = $"{nameof(IsItDarkNow)}()",
        Action = "_lightSwitch.Dim(25)")]
    [Transition(nameof(PresenceDetected), nameof(LightOff), Condition = $"!{nameof(IsItDarkNow)}()")]
    [Transition(nameof(ButtonPressed), nameof(ManualOn))]
    [InitialState]
    private partial void LightOff();

    [OnEntry("_lightSwitch.ToOn()")]
    [Transition(nameof(NoPresenceDetected), nameof(AutoToOff))]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [State]
    private partial void LightOn();

    [OnEntry("_lightSwitch.ToOn()")]
    [Transition(nameof(NoPresenceDetected), nameof(AutoManualToOff))]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [State]
    private partial void ManualOn();

    [OnEntry("_lightSwitch.ToOff()")]
    [OnEntry("_lightSwitch.Dim(0)")]
    [Transition(nameof(ButtonPressed), nameof(ManualOn))]
    [TransitionAfter("PT5S", nameof(LightOff), Condition = "true")]
    [TransitionAfter("0:00:10", nameof(LightOff))]
    [State]
    private partial void ManualToOff();

    [Transition(nameof(PresenceDetected), nameof(LightOn), Condition = $"{nameof(IsItDarkNow)}()")]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [TransitionAfter("0:00:05", nameof(LightOff))]
    [State]
    private partial void AutoToOff();

    [Transition(nameof(PresenceDetected), nameof(ManualOn))]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [TransitionAfter("PT20M20.4S", nameof(LightOff))]
    [State]
    private partial void AutoManualToOff();

    [Trigger]
    public partial void ButtonPressed();

    [Trigger]
    public partial void PresenceDetected();

    [Trigger]
    public partial void NoPresenceDetected();

    public bool IsItDarkNow()
        => _lightSensor.SensorValue < 2.9m;
}
