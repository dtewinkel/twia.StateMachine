using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator;

public static class StateMachineGeneratorDiagnostics
{
    private static readonly DiagnosticDescriptor _classMustBePartialDescriptor = new(
        "SMG0001", 
        "Class must be partial", 
        "The class '{0}' must be a partial type", 
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "To be able to generate the state machine logic, the class must be partial.");

    private static readonly DiagnosticDescriptor _multipleInitialStatesDescriptor = new(
        "SMG0002",
        "Only one InitialState allowed",
        "The method '{0}' is marked as InitialState, but method '{1}' is als marked as InitialState",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "Only a single state can be marked with InitialStateAttribute.");

    private static readonly DiagnosticDescriptor _noInitialStateDescriptor = new(
        "SMG0003",
        "At least one InitialState must be defined",
        "The class '{0}' must have one State that is marked as InitialState",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "A single method with the StateAttribute must be marked with an InitialStateAttribute.");

    private static readonly DiagnosticDescriptor _methodCannotBeTriggerAndStateDescriptor = new(
        "SMG0004",
        "Method cannot be a trigger and a state at the same time",
        "The method '{0}' can not be a trigger and a state at the same time",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "A method can only be a trigger (with the TriggerAttribute) or a state (with the StateAttribute).");

    private static readonly DiagnosticDescriptor _methodMustBeAStateDescriptor = new(
        "SMG0005",
        "Method must be declared as a state",
        "The method '{0}' must be declared as a state to be able to use any of the transition or initial state attributes",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "When any of the transition attributes (OnEntryAttribute, OnExitAttribute, TransitionAttribute, or TransitionAfterAttribute), or the InitialStateAttribute is used on a method, " +
            "then that method must be marker with the StateAttribute.");

    private static readonly DiagnosticDescriptor _triggerCannotHaveStateAttributesDescriptor = new(
        "SMG0006",
        "Trigger can not have state attributes",
        "The method '{0}' is declared as a trigger and cannot use any of the transition or initial state attributes",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "A method marked as a trigger with the TriggerAttribute cannot have any of the transition attributes (OnEntryAttribute, OnExitAttribute, TransitionAttribute, or TransitionAfterAttribute), or the InitialStateAttribute applied.");

    private static readonly DiagnosticDescriptor _methodMustBePartialDescriptor = new(
        "SMG0007",
        "Method must be partial",
        "The method '{0}' must be a partial method",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "To be able to generate the state machine logic, the method must be partial.");

    private static readonly DiagnosticDescriptor _methodMustHaveVoidReturnTypeDescriptor = new(
        "SMG0008",
        "Method must have void return",
        "The method '{0}' must have a void return type",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "The generated method will not return anything and therefor must have a void return type.");

    private static readonly DiagnosticDescriptor _methodMustHaveNoParametersDescriptor = new(
        "SMG0009",
        "Method must have no parameters",
        "The method '{0}' must not have parameters",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "The generated method will use any parameters and therefor should not provide any.");

    private static readonly DiagnosticDescriptor _triggerMustBeDefinedDescriptor = new(
        "SMG0010",
        "Trigger must be defined",
        "The trigger '{0}' used for state method '{1}' must be defined",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "All triggers mentioned in Transitions must be defined as a trigger method.");

    private static readonly DiagnosticDescriptor _stateMustBeDefinedDescriptor = new(
        "SMG0011",
        "State must be defined",
        "The target State '{0}' used for state method '{1}' must be defined",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "All stated mentioned in Transitions must be defined as a state method.");

    private static readonly DiagnosticDescriptor _timeSpanMustBeValidDescriptor = new(
        "SMG0012",
        "TimeSpan value must be valid",
        "The time span '{0}' used for state method '{1}' must be be a valid TimeSpan parsable value",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "All time span values must be valid.");

    private static readonly DiagnosticDescriptor _methodMustHaveTaskReturnTypeDescriptor = new(
        "SMG0013",
        "Method must have a Task return type",
        "The method '{0}' must have a Task return type",
        "Generator",
        DiagnosticSeverity.Error,
        true,
        "The generated method will not return any data, but is async, and therefor must have a Task return type.");

    public static Diagnostic ClassMustBePartialDiagnostics(ClassDeclarationSyntax declaration)
    {
        return Diagnostic.Create(_classMustBePartialDescriptor, declaration.Identifier.GetLocation(), declaration.Identifier.ToString());
    }

    public static Diagnostic MultipleInitialStatesDiagnostics(MethodDeclarationSyntax initialStateNode, string declarationName)
    {
        return Diagnostic.Create(_multipleInitialStatesDescriptor, initialStateNode.Identifier.GetLocation(), initialStateNode.Identifier.ToString(), declarationName);
    }

    public static Diagnostic NoInitialStateDiagnostics(ClassDeclarationSyntax declaration)
    {
        return Diagnostic.Create(_noInitialStateDescriptor, declaration.Identifier.GetLocation(), declaration.Identifier.ToString());
    }

    public static Diagnostic MethodCannotBeTriggerAndState(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_methodCannotBeTriggerAndStateDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }

    public static Diagnostic MethodMustBeAState(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_methodMustBeAStateDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }

    public static Diagnostic TriggerCannotHaveStateAttributes(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_triggerCannotHaveStateAttributesDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }

    public static Diagnostic MethodMustBePartial(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_methodMustBePartialDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }
    
    public static Diagnostic MethodMustHaveVoidReturnType(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_methodMustHaveVoidReturnTypeDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }

    public static Diagnostic MethodMustHaveTaskReturnType(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_methodMustHaveTaskReturnTypeDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }

    public static Diagnostic MethodMustHaveNoParameters(MethodDeclarationSyntax declarationMethodNode)
    {
        return Diagnostic.Create(_methodMustHaveNoParametersDescriptor, declarationMethodNode.Identifier.GetLocation(), declarationMethodNode.Identifier.ToString());
    }

    public static Diagnostic TriggerMustBeDefined(MethodDeclarationSyntax declarationNode, string trigger)
    {
        return Diagnostic.Create(_triggerMustBeDefinedDescriptor, declarationNode.Identifier.GetLocation(), trigger, declarationNode.Identifier.ToString());
    }

    public static Diagnostic StateMustBeDefined(MethodDeclarationSyntax declarationNode, string state)
    {
        return Diagnostic.Create(_stateMustBeDefinedDescriptor, declarationNode.Identifier.GetLocation(), state, declarationNode.Identifier.ToString());
    }

    public static Diagnostic TimeSpanMustBeValid(MethodDeclarationSyntax declarationNode, string timeSpan)
    {
        return Diagnostic.Create(_timeSpanMustBeValidDescriptor, declarationNode.Identifier.GetLocation(), timeSpan, declarationNode.Identifier.ToString());
    }
}