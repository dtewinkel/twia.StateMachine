using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twia.StateMachine.CodeGenerator.Builders;
using Twia.StateMachine.CodeGenerator.Declarations;
using Twia.StateMachine.CodeGenerator.Validators;

namespace Twia.StateMachine.CodeGenerator;

[Generator(LanguageNames.CSharp)]
public class StateMachineIncrementalCodeGenerator: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var stateMachineLogicDeclaration = context.SyntaxProvider
                .ForAttributeWithMetadataName(StateMachineAttributeNames.StateMachineAttributeName, SyntaxProviderPredicate, Transform);

        context.RegisterSourceOutput(stateMachineLogicDeclaration, AddSource);
    }

    private static void AddSource(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var validator = new StateMachineValidator();
        validator.IsDeclarationValid(context, declaration);
        StateMachineSourceBuilder.AddSource(context, declaration);
    }


    private static bool SyntaxProviderPredicate(SyntaxNode syntaxNode, CancellationToken _)
        => syntaxNode is ClassDeclarationSyntax;

    private static StateMachineDeclaration Transform(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        var classNode = (ClassDeclarationSyntax)context.TargetNode;
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        return new StateMachineDeclaration(classNode, classSymbol);
    }
}