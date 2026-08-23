using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public partial record InternalTransitionDeclaration : TransitionDeclaration
{
    public InternalTransitionDeclaration(AttributeData attributeData)
    {
        TransitionType = TransitionType.Internal;
        Name = TransitionType.ToString();
        Trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        TargetState = "self"; 
        Action = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
    }
}
