using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public abstract record ParentDeclaration : Declaration
{
    protected ParentDeclaration(CSharpSyntaxNode node) : base(node)
    {
    }

    public abstract string? FullNamespaceName { get; }

    public abstract string HintNameForSource { get; }
}