using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Validators;

public class AsyncStateMachineValidator : BaseValidator
{
    protected override bool MethodsSignaturesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var success = base.MethodsSignaturesAreCorrect(context, declaration);
        foreach (var declarationMethod in declaration.Methods)
        { 
            if (declarationMethod.ReturnType is not CommonTypeNames.Task)
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustHaveTaskReturnType((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }

            var parameterCount = declarationMethod.Parameters.Count;
            if (parameterCount > 0)
            {
                if (parameterCount == 1 &&
                    declarationMethod.Parameters[0].ParameterType == CommonTypeNames.CancellationToken)
                {
                    continue;
                }
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustHaveNoParameters((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }
        }
        return success;
    }
}