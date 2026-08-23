using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record InternalAfterDeclaration : TransitionDeclaration
{
    public InternalAfterDeclaration(string stateName, int index, AttributeData attributeData)
    {
        TransitionType = TransitionType.InternalAfter;
        Name = $"{stateName}After{index}";
        Trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        TargetState = "self";
        Action = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
    }
}