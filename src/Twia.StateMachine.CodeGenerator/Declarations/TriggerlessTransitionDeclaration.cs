using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record TriggerlessTransitionDeclaration : TransitionDeclaration
{
    public TriggerlessTransitionDeclaration(AttributeData attributeData)
    {
        TransitionType = TransitionType.Triggerless;
        Name = TransitionType.ToString();
        Trigger = TransitionType.ToString();
        TargetState = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        Action = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Action").Value.Value?.ToString();
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
    }
}