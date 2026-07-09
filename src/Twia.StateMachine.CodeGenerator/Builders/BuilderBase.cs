namespace Twia.StateMachine.CodeGenerator.Builders;

public class BuilderBase
{
    public virtual bool IsEnabled => true;

    public virtual bool AddTypes() => false;

    public virtual bool AddConstants() => false;

    public virtual bool AddFields() => false;

    public virtual bool AddPublicProperties() => false;

    public virtual bool AddPublicMethods() => false;

    public virtual bool AddPrivateMethods() => false;

    public virtual bool AddEvents() => false;
}