using Generator.Equals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public sealed partial record MethodDeclaration : Declaration
{
    public MethodDeclaration(MethodDeclarationSyntax node, IList<AttributeData> attributes) : base(node)
    {
        Name = node.Identifier.ToString();
        Modifiers = node.Modifiers.ToString();
        ReturnType = node.ReturnType.ToString();

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
            Parameters.Add(parameter.ToString());
        }
    }

    public string Name { get; }

    public string Modifiers { get; }

    public string ReturnType { get; }

    public bool IsPartial { get; }
    
    public bool IsState { get; }

    public bool IsTrigger { get; }

    public bool IsInitial { get; }

    [OrderedEquality]
    public List<string> Parameters { get; } = [];

    [OrderedEquality]
    public List<TransitionDeclaration> Transitions { get; } = [];
}