using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record  OnEntryTransitionDeclaration : TransitionDeclaration
{
    public OnEntryTransitionDeclaration(AttributeData attributeData)
    {
        TransitionType = TransitionType.OnEntry;
        Name = TransitionType.ToString();
        Trigger = TransitionType.ToString();
        TargetState = "self";
        Action = attributeData.ConstructorArguments[0].Value?.ToString();
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
    }
}