using Generator.Equals;
using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record AfterDelayTransitionDeclaration : TransitionDeclaration
{
    public AfterDelayTransitionDeclaration(string stateName, int index, AttributeData attributeData)
    {
        TransitionType = TransitionType.AfterDelay;
        Name = $"{stateName}After{index}";
        Trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        TargetState = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
        Action = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Action").Value.Value?.ToString();
    }
}