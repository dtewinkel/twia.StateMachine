using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Validators;

public class StateMachineValidator : BaseValidator
{
    protected override bool MethodsSignaturesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var success = base.MethodsSignaturesAreCorrect(context, declaration);
        foreach (var declarationMethod in declaration.Methods)
        { 
            if (declarationMethod.ReturnType is not "void")
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustHaveVoidReturnType((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }
            if (declarationMethod.Parameters.Count > 0)
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustHaveNoParameters((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }
        }
        return success;
    }
}