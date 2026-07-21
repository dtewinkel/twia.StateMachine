using System.Security.Cryptography.X509Certificates;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Async;

public class StatesManagementBuilder : BuilderBase
{
    private readonly CSharpDocumentWriter _document;
    private readonly StatesBuilder _statesBuilder;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly AfterTransitionsBuilder _afterTransitionsBuilder;
    private readonly ObservableBuilder _observableBuilder;

    public StatesManagementBuilder(CSharpDocumentWriter document,
        StatesBuilder statesBuilder, TriggersBuilder triggersBuilder,
        AfterTransitionsBuilder afterTransitionsBuilder, ObservableBuilder observableBuilder)
    {
        _document = document;
        _statesBuilder = statesBuilder;
        _triggersBuilder = triggersBuilder;
        _afterTransitionsBuilder = afterTransitionsBuilder;
        _observableBuilder = observableBuilder;
    }

    public override bool IsEnabled => _statesBuilder.IsEnabled || _triggersBuilder.IsEnabled;

    public override bool AddPublicMethods()
    {
        AddInitializeMethod();
        return true;
    }

    private void AddInitializeMethod()
    {
        var initialStateName = _statesBuilder.InitialStateName;
        var hasInitialState = initialStateName is not null;
        var asyncKeyword = hasInitialState ? "async " : string.Empty;
        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Initialize the state machine before it is used.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine("/// <remarks>");
        _document.WriteLine("/// The state machine must be initialize once (and only once) before any of the other generated methods and properties can be used.");
        _document.WriteLine("/// </remarks>");
        _document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        _document.WriteLine("/// InitializeStateMachine() can only be called one in the life of a state machine");
        _document.WriteLine("/// </exception>");
        _document.WriteLine($"public {asyncKeyword}{CommonTypeNames.Task} InitializeStateMachineAsync({CommonTypeNames.CancellationToken} cancellationToken = default)");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"if ({_statesBuilder.StateFieldName} != {_statesBuilder.UndefinedStateName})");
        _document.WriteLineBlockOpen();
        _document.WriteLine("""throw new global::System.InvalidOperationException("'InitializeStateMachine()' can only be called once in the lifecycle of an instance.");""");
        _document.WriteLineBlockClose();
        _document.WriteLineNoTabs();
        if (initialStateName is not null)
        {
            _document.WriteLine($"// Move to initial state '{initialStateName}'.");
            _document.WriteLine($"await {_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{initialStateName}, \"Initial\", cancellationToken);");
        }
        else
        {
            _document.WriteLine($"return {CommonTypeNames.Task}.CompletedTask;");
        }
        _document.WriteLineBlockClose();
    }

    public override bool AddPrivateMethods()
    {
        AddInvokeTriggerMethod();
        _document.WriteLineNoTabs();
        AddEnterStateMethod();
        if (_statesBuilder.HasStates)
        {
            _document.WriteLineNoTabs();
            AddStateMethods();
        }

        return true;
    }

    private void AddEnterStateMethod()
    {
        const string stateParameterName = "state";
        const string reasonParameterName = "reason";
        _document.WriteLine($"private async {CommonTypeNames.Task} {_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName} {stateParameterName}, string {reasonParameterName}, {CommonTypeNames.CancellationToken} cancellationToken = default)");
        _document.WriteLineBlockOpen();
        _afterTransitionsBuilder.AddClearTimers();
        _observableBuilder.AddObserveStateChange(stateParameterName, reasonParameterName);
        _document.WriteLine($"{_statesBuilder.StateFieldName} = {stateParameterName};");
        _document.WriteLine($"await {_triggersBuilder.InvokeTriggerMethodName}({_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName}, cancellationToken);");
        _document.WriteLineBlockClose();
    }

    private void AddInvokeTriggerMethod()
    {
        var hasStates = _statesBuilder.HasStates;
        var asyncKeyword = hasStates ? "async " : string.Empty;
        _document.WriteLine($"private {asyncKeyword}{CommonTypeNames.Task} {_triggersBuilder.InvokeTriggerMethodName}({_triggersBuilder.TriggerEnumTypeName} trigger, {CommonTypeNames.CancellationToken} cancellationToken = default)");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{_triggersBuilder.LastTriggerFieldName} = trigger;");
        _document.WriteLineNoTabs();
        if (hasStates)
        {
            _document.WriteLine($"switch ({_statesBuilder.StateFieldName})");
            _document.WriteLineBlockOpen();
            var first = true;
            foreach (var stateName in _statesBuilder.StateNames)
            {
                var stateMethod = _statesBuilder.GetState(stateName);
                var cancellationToken = stateMethod.CancellationTokenParameterName != null ? "cancellationToken" : "";
                first = _document.WriteSeparatorLine(first);
                _document.WriteLine($"case {_statesBuilder.StateFullTypeName}.{stateName}:");
                _document.Indent++;
                _document.WriteLine($"await {stateName}({cancellationToken});");
                _document.WriteLine("break;");
                _document.Indent--;
            }

            _document.WriteLineBlockClose();
        }
        else
        {
            _document.WriteLine($"return {CommonTypeNames.Task}.CompletedTask;");
        }

        _document.WriteLineBlockClose();
    }

    private void AddStateMethods()
    {
        var firstStateMethod = true;
        foreach (var stateName in _statesBuilder.StateNames)
        {
            firstStateMethod = _document.WriteSeparatorLine(firstStateMethod);

            using var methodBody = new CSharpDocumentWriter(1000);
            var state = _statesBuilder.GetState(stateName);

            var onEntryTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnEntry).ToList();
            var hasEntryTransitions = onEntryTransitions.Count > 0;

            var triggerTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnTrigger).ToList();
            var hasTriggerTransactions = triggerTransitions.Count > 0;

            var hasAfterTransitions = _afterTransitionsBuilder.HasAfterTransitions(stateName);

            var onExitCall = CreateOnExitCall(methodBody, state, null);

            if (hasEntryTransitions || hasTriggerTransactions || hasAfterTransitions)
            {
                methodBody.WriteLine($"switch ({_triggersBuilder.LastTriggerFieldName})");
                methodBody.WriteLineBlockOpen();
                var first = true;

                if (hasEntryTransitions || hasAfterTransitions)
                {
                    first = methodBody.WriteSeparatorLine(first);

                    methodBody.WriteLine(
                        $"case {_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName}:");
                    methodBody.Indent++;

                    _afterTransitionsBuilder.AddStartTimers(methodBody, stateName);
                    foreach (var transitionDeclaration in onEntryTransitions)
                    {
                        methodBody.WriteConditionAndAction(transitionDeclaration);
                    }

                    methodBody.WriteLine("break;");
                    methodBody.Indent--;
                }

                if (hasTriggerTransactions)
                {
                    var triggersGrouped = triggerTransitions.GroupBy(trigger => trigger.Trigger);
                    foreach (var trigger in triggersGrouped)
                    {
                        first = methodBody.WriteSeparatorLine(first);
                        methodBody.WriteLine($"case {_triggersBuilder.TriggerEnumTypeName}.{trigger.Key}:");
                        methodBody.Indent++;
                        foreach (var transition in trigger.ToList())
                        {
                            methodBody.WriteConditionActionAndTransition(transition, onExitCall,
                                (document, declaration) =>
                                {
                                    document.WriteLine($"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"Trigger: {trigger.Key}\");");
                                }
                            );
                        }

                        methodBody.WriteLine("break;");
                        methodBody.Indent--;
                    }
                }

                if (hasAfterTransitions)
                {
                    _afterTransitionsBuilder.AddTimerTransitions(methodBody, stateName, first, onExitCall);
                }
                methodBody.WriteLineBlockClose();
            }

            var parameters = string.Join(", ",
                state.Parameters.Select(p =>
                    $"{p.Modifiers}{(string.IsNullOrEmpty(p.Modifiers) ? "" : " ")}{p.ParameterType} {p.Name}"));

            var methodReturnType = methodBody.GetMethodReturnType();
            //var methodDeclarationAuto = ClassCommonBuilder.ToMethodDeclaration(methodReturnType, state.Name, parameters, false);
            //methodBody.WriteLine($"// {state.Modifiers} {methodDeclarationAuto};");

            //var returnType = methodBody.GetMethodReturnType();
            // var methodDeclaration = $"{state.Modifiers} {ClassCommonBuilder.ToMethodDeclaration(returnType, state.Name, parameters, false)}";
            var methodDeclaration = $"{state.Modifiers} async {state.ReturnType} {state.Name}({parameters})";
            _document.WriteMethod(methodDeclaration, methodBody);
        }
    }

    private string? CreateOnExitCall(CSharpDocumentWriter document, MethodDeclaration state, string? cancellationParameter)
    {
        var onExitTransitions = state.Transitions
            .Where(transition => transition.TransitionType == TransitionType.OnExit).ToList();
        var hasExitTransactions = onExitTransitions.Count > 0;

        if (hasExitTransactions)
        {
            using var methodBodyDocument = new CSharpDocumentWriter(1000);

            foreach (var transitionDeclaration in onExitTransitions)
            {
                methodBodyDocument.WriteConditionAndAction(transitionDeclaration);
            }

            var onExitCancellationParameter = cancellationParameter is not null ? "cancellationToken" : null;

            var methodReturnType = methodBodyDocument.GetMethodReturnType();
            var methodDeclaration = ClassCommonBuilder.ToMethodDeclaration(methodReturnType, "OnExit", onExitCancellationParameter);
            document.WriteMethod(methodDeclaration, methodBodyDocument);
            document.WriteLineNoTabs();

            return ClassCommonBuilder.ToMethodCall(methodReturnType, "OnExit", cancellationParameter);
        }

        return null;
    }
}
