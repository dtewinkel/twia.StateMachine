using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public sealed partial record StateMachineDeclaration : ClassDeclaration
{
    public StateMachineDeclaration(ClassDeclarationSyntax node, INamedTypeSymbol symbol) : base(node)
    {
        var stateMachineAttribute = symbol
            .GetAttributes()
            .Single(attribute => attribute.GetFullName() == StateMachineAttributeNames.StateMachineAttributeName);
        StateAccessible = stateMachineAttribute.NamedArguments.FirstOrDefault(kv => kv.Key == "StateAccessible").Value.Value as bool? ?? true;
        Observable = stateMachineAttribute.NamedArguments.FirstOrDefault(kv => kv.Key == "Observable").Value.Value as bool? ?? false;

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }
            var attributes = method
                .GetAttributes()
                .Where(attribute => StateMachineAttributeNames.AllMethodAttributeNames.Contains(attribute.GetFullName()))
                .ToList();
            if (attributes.Count == 0)
            {
                continue;
            }
            var methodNode = node.Members
                .First(nodeMember => nodeMember is MethodDeclarationSyntax methodDeclaration
                                     && methodDeclaration.Identifier.ToString() == method.Name);
            Methods.Add(new MethodDeclaration((MethodDeclarationSyntax)methodNode, method, attributes));
        }
    }

    [SequenceEquality]
    public List<MethodDeclaration> Methods { get; } = [];

    public bool StateAccessible { get; }

    public bool Observable { get; }

    public MethodDeclaration GetMethod(string name) => Methods.First(methodDeclaration => methodDeclaration.Name == name);
}