namespace Twia.StateMachine.CodeGenerator.Builders;

public interface ITriggersProvider
{
    bool IsEnabled { get; }

    string[] GetTriggerNames();
}