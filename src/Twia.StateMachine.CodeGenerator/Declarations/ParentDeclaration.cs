using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public abstract partial record ParentDeclaration : Declaration
{
    protected ParentDeclaration(CSharpSyntaxNode node) : base(node)
    {
    }

    public abstract string? FullNamespaceName { get; }

    public abstract string HintNameForSource { get; }
}