using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public sealed partial record ParameterDeclaration : Declaration
{
    public ParameterDeclaration(ParameterSyntax node, IParameterSymbol parameter) : base(node)
    {
        Name = node.Identifier.ToString();
        Modifiers = node.Modifiers.ToString();
        ParameterType = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public string Name { get; }

    public string Modifiers { get; }

    public string ParameterType { get; }

    public bool IsCancellationToken => ParameterType == CommonTypeNames.CancellationToken;
}