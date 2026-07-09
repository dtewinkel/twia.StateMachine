namespace Twia.StateMachine.Example;

public interface ILightSwitch
{
    void ToOn();

    void ToOff();

    void Dim(int percentage);
}