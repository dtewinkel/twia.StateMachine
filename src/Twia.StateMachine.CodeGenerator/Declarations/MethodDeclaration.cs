using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public sealed partial record MethodDeclaration : Declaration
{
    public MethodDeclaration(MethodDeclarationSyntax node, IMethodSymbol method, IList<AttributeData> attributes) : base(node)
    {
        Name = node.Identifier.ToString();
        Modifiers = node.Modifiers.ToString();
        ReturnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        IsPartial = node.Modifiers.Any(SyntaxKind.PartialKeyword);
        IsState = attributes.Any(attribute => attribute.GetFullName() == StateMachineAttributeNames.StateAttributeName 
                                              || attribute.GetFullName() == StateMachineAttributeNames.InitialStateAttributeName);
        IsTrigger = attributes.Any(attribute => attribute.GetFullName() == StateMachineAttributeNames.TriggerAttributeName);
        IsInitial = attributes.Any(attribute => attribute.GetFullName() == StateMachineAttributeNames.InitialStateAttributeName);

        var index = 1;

        foreach (var attributeData in attributes)
        {
            switch (attributeData.GetFullName())
            {
                case StateMachineAttributeNames.OnEntryAttributeName:
                    Transitions.Add(new OnEntryTransitionDeclaration(attributeData));
                    break;

                case StateMachineAttributeNames.OnExitAttributeName:
                    Transitions.Add(new OnExitTransitionDeclaration(attributeData));
                    break;

                case StateMachineAttributeNames.TransitionAttributeName:
                    Transitions.Add(new OnTriggerTransitionDeclaration(attributeData));
                    break;

                case StateMachineAttributeNames.TransitionAfterAttributeName:
                    Transitions.Add(new AfterDelayTransitionDeclaration(Name, index++, attributeData));
                    break;
            }
        }

        foreach (var parameter in node.ParameterList.Parameters)
        {
            var parameterSymbol = method.Parameters.First(p => p.Name == parameter.Identifier.ToString());
            Parameters.Add(new ParameterDeclaration(parameter, parameterSymbol));
        }
    }

    public string Name { get; }

    public string Modifiers { get; }

    public string ReturnType { get; }

    public bool IsPartial { get; }
    
    public bool IsState { get; }

    public bool IsTrigger { get; }

    public bool IsInitial { get; }

    [SequenceEquality]
    public List<ParameterDeclaration> Parameters { get; } = [];

    [SequenceEquality]
    public List<TransitionDeclaration> Transitions { get; } = [];

    public string? CancellationTokenParameterName => Parameters.FirstOrDefault(p => p.IsCancellationToken)?.Name;
}