using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public sealed partial record ContainingClassDeclaration : ParentDeclaration
{
    public ContainingClassDeclaration(ClassDeclarationSyntax classDeclaration) : base(classDeclaration)
    {
        ClassDeclaration = new ClassDeclaration(classDeclaration);
    }

    public ClassDeclaration ClassDeclaration { get; }

    [IgnoreEquality]
    public ParentDeclaration? Parent => ClassDeclaration.Parent;

    [IgnoreEquality]
    public override string? FullNamespaceName => Parent?.FullNamespaceName;

    [IgnoreEquality]
    public override string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{ClassDeclaration.Name}";
}