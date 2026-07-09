using System.Threading;
using System.Threading.Tasks;

namespace Twia.StateMachine.Example;

[AsyncStateMachine(Observable = true, StateAccessible = true)]
public partial class AsyncLightingStateMachine
{
    private readonly IAsyncLightSwitch _lightSwitch;
    private readonly IAsyncLightSensor _lightSensor;

    public AsyncLightingStateMachine(IAsyncLightSwitch lightSwitch, IAsyncLightSensor lightSensor)
    {
        _lightSwitch = lightSwitch;
        _lightSensor = lightSensor;
    }

    [OnEntry("await _lightSwitch.ToOffAsync()")]
    [Transition(nameof(PresenceDetected), nameof(LightOn), Condition = $"await {nameof(IsItDarkNow)}()",
        Action = "await _lightSwitch.DimAsync(25)")]
    [Transition(nameof(PresenceDetected), nameof(LightOff), Condition = $"! await {nameof(IsItDarkNow)}()")]
    [Transition(nameof(ButtonPressed), nameof(ManualOn))]
    [InitialState]
    private partial Task LightOff();

    [OnEntry("await _lightSwitch.ToOnAsync()")]
    [Transition(nameof(NoPresenceDetected), nameof(AutoToOff))]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [State]
    private partial Task LightOn();

    [OnEntry("await _lightSwitch.ToOnAsync()")]
    [Transition(nameof(NoPresenceDetected), nameof(AutoManualToOff))]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [State]
    private partial Task ManualOn();

    [OnEntry("await _lightSwitch.ToOffAsync()")]
    [OnEntry("await _lightSwitch.DimAsync(0)")]
    [Transition(nameof(ButtonPressed), nameof(ManualOn))]
    [TransitionAfter("0:00:05", nameof(LightOff), Condition = "true")]
    [TransitionAfter("0:00:10", nameof(LightOff))]
    [State]
    private partial Task ManualToOff();

    [Transition(nameof(PresenceDetected), nameof(LightOn), Condition = $"await {nameof(IsItDarkNow)}()")]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [TransitionAfter("0:00:05", nameof(LightOff))]
    [State]
    private partial Task AutoToOff();

    [Transition(nameof(PresenceDetected), nameof(ManualOn))]
    [Transition(nameof(ButtonPressed), nameof(ManualToOff))]
    [TransitionAfter("0:20:00", nameof(LightOff))]
    [State]
    private partial Task AutoManualToOff(CancellationToken cancellationToken);

    [Trigger]
    public partial Task ButtonPressed();

    [Trigger]
    public partial Task PresenceDetected(CancellationToken cancellationToken);

    [Trigger]
    public partial Task NoPresenceDetected();

    public async Task<bool> IsItDarkNow()
        => await _lightSensor.GetSensorValueAsync() < 2.9m;
}
