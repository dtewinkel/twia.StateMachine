using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Validators;

public class BaseValidator
{
    public virtual bool IsDeclarationValid(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var declarationIsPartial = DeclarationIsPartial(context, declaration);
        if (declaration.Methods.Count == 0)
        {
            return declarationIsPartial;
        }

        var initialStateIsValid = InitialStateIsValid(context, declaration);
        var methodsAreStateOrTrigger = MethodsAreStateOrTrigger(context, declaration);
        var methodsSignaturesAreCorrect = MethodsSignaturesAreCorrect(context, declaration);
        var triggerNamesAreCorrect = TriggerNamesAreCorrect(context, declaration);
        var afterTimeSpansAreCorrect = AfterTimeSpansAreCorrect(context, declaration);
        var stateNamesAreCorrectNamesAreCorrect = StateNamesAreCorrect(context, declaration);
        return declarationIsPartial 
               && initialStateIsValid 
               && methodsAreStateOrTrigger 
               && methodsSignaturesAreCorrect
               && triggerNamesAreCorrect
               && afterTimeSpansAreCorrect
               && stateNamesAreCorrectNamesAreCorrect;
    }

    protected virtual bool StateNamesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var states = declaration.Methods.Where(method => method.IsState).ToList();
        var stateNames = states.Select(method => method.Name).ToArray();
        var success = true;

        foreach (var state in states)
        {
            var transitions =
                state.Transitions.Where(transition => transition.TransitionType is TransitionType.OnTrigger or TransitionType.AfterDelay);
            foreach (var transitionDeclaration in transitions)
            {
                var targetState = transitionDeclaration.TargetState;
                if (!stateNames.Contains(targetState))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.StateMustBeDefined((MethodDeclarationSyntax)state.Node, targetState));
                    success = false;
                }
            }
        }

        return success;
    }

    protected virtual bool TriggerNamesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var triggerNames = declaration.Methods
            .Where(method => method.IsTrigger)
            .Select(method => method.Name)
            .ToArray();

        var states = declaration.Methods.Where(method => method.IsState);

        var success = true;
        foreach (var state in states)
        {
            var transitions =
                state.Transitions.Where(transition => transition.TransitionType == TransitionType.OnTrigger);
            foreach (var transitionDeclaration in transitions)
            {
                var trigger = transitionDeclaration.Trigger;
                if (!triggerNames.Contains(trigger))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.TriggerMustBeDefined((MethodDeclarationSyntax)state.Node, trigger));
                    success = false;
                }
            }
        }

        return success;
    }

    protected virtual bool AfterTimeSpansAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var states = declaration.Methods.Where(method => method.IsState);
        var success = true;
        foreach (var state in states)
        {
            var transitions =
                state.Transitions.Where(transition => transition.TransitionType == TransitionType.AfterDelay);
            foreach (var transitionDeclaration in transitions)
            {
                var trigger = transitionDeclaration.Trigger;
                
                if (!TimeSpan.TryParse(trigger, out _))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.TimeSpanMustBeValid((MethodDeclarationSyntax)state.Node, trigger));
                    success = false;
                }
            }
        }

        return success;
    }

    protected virtual bool DeclarationIsPartial(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        if (!declaration.IsPartial)
        {
            context.ReportDiagnostic(StateMachineGeneratorDiagnostics.ClassMustBePartialDiagnostics((ClassDeclarationSyntax)declaration.Node));
            return false;
        }

        return true;
    }

    protected virtual bool MethodsAreStateOrTrigger(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var success = true;
        foreach (var declarationMethod in declaration.Methods)
        {
            if (declarationMethod is { IsState: true, IsTrigger: true })
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodCannotBeTriggerAndState((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }
            if (declarationMethod is { IsState: false, IsTrigger: false } && (declarationMethod.Transitions.Count > 0 || declarationMethod.IsInitial))
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustBeAState((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }
            if (declarationMethod is { IsTrigger: true } && (declarationMethod.Transitions.Count > 0 || declarationMethod.IsInitial))
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.TriggerCannotHaveStateAttributes((MethodDeclarationSyntax)declarationMethod.Node));
                success = false;
            }
        }
        return success;
    }

    protected virtual bool MethodsSignaturesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var success = true;
        foreach (var declarationMethod in declaration.Methods)
        {
            if (declarationMethod is not { IsPartial: false })
            {
                continue;
            }
            context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustBePartial((MethodDeclarationSyntax)declarationMethod.Node));
            success = false;
        }
        return success;
    }

    protected virtual bool InitialStateIsValid(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var initialStates = declaration.Methods.Where(method => method.IsInitial).ToArray();
        switch (initialStates.Length)
        {
            case 0:
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.NoInitialStateDiagnostics((ClassDeclarationSyntax)declaration.Node));
                return false;

            case > 1:
            {
                var initialStateName = initialStates[0].Name;
                foreach (var initialState in initialStates.Skip(1))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MultipleInitialStatesDiagnostics((MethodDeclarationSyntax)initialState.Node, initialStateName));
                }
                return false;
            }
        }

        return true;
    }
}