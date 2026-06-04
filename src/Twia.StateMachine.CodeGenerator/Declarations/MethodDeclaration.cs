using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public sealed record MethodDeclaration : Declaration
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

    public List<string> Parameters { get; } = [];

    public List<TransitionDeclaration> Transitions { get; } = [];

    public bool Equals(MethodDeclaration? other)
    {
        if (other is null)
        {
            return false;
        }

        return Modifiers == other.Modifiers
               && Name == other.Name
               && ReturnType == other.ReturnType
               && IsPartial == other.IsPartial
               && IsState == other.IsState
               && IsTrigger == other.IsTrigger
               && IsInitial == other.IsInitial
               && Parameters.SequenceEqual(other.Parameters)
               && Transitions.SequenceEqual(other.Transitions);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Modifiers);
        hash.Add(ReturnType);
        hash.Add(IsInitial);
        hash.Add(IsState);
        hash.Add(IsPartial);
        hash.Add(IsTrigger);

        foreach (var parameter in Parameters)
        {
            hash.Add(parameter);
        }

        foreach (var transition in Transitions)
        {
            hash.Add(transition);
        }

        return hash.ToHashCode();
    }
}