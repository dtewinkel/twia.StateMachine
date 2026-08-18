namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public abstract partial record TransitionDeclaration
{
    public TransitionType TransitionType { get; protected set; }

    public string Name { get; protected set; } = null!;

    public string Trigger { get; protected set; } = null!;

    public string TargetState { get; protected set; } = null!;

    public string? Condition { get; protected set; }
    
    public string? Action { get; protected set; }
}
