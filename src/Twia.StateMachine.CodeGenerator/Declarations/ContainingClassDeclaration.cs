using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public sealed record ContainingClassDeclaration : ParentDeclaration
{
    public ContainingClassDeclaration(ClassDeclarationSyntax classDeclaration) : base(classDeclaration)
    {
        ClassDeclaration = new ClassDeclaration(classDeclaration);
    }

    public ClassDeclaration ClassDeclaration { get; }

    public ParentDeclaration? Parent => ClassDeclaration.Parent;

    public override string? FullNamespaceName => Parent?.FullNamespaceName;

    public override string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{ClassDeclaration.Name}";

    public bool Equals(ContainingClassDeclaration? other)
    {
        return other is not null
               && ClassDeclaration.Equals(other.ClassDeclaration);
    }

    public override int GetHashCode() => ClassDeclaration.GetHashCode();
}