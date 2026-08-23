using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record InternalAfterEveryDeclaration : TransitionDeclaration
{
    public string InitialDelay { get; protected set; } = null!;
    
    public InternalAfterEveryDeclaration(string stateName, int index, AttributeData attributeData)
    {
        TransitionType = TransitionType.InternalAfterEvery;
        Name = $"{stateName}After{index}";
        Trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        TargetState = "self";
        Action = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
        InitialDelay = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "InitialDelay").Value.Value?.ToString() ?? Trigger;
    }
}