using Generator.Equals;
using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record OnTriggerTransitionDeclaration : TransitionDeclaration
{
    public OnTriggerTransitionDeclaration(AttributeData attributeData)
    {
        TransitionType = TransitionType.OnTrigger;
        var trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        Name = trigger;
        Trigger = trigger;
        TargetState = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
        Action = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Action").Value.Value?.ToString();
    }
}