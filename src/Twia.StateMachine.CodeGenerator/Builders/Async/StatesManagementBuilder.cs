using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using System.Xml.Linq;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Async;

public class StatesManagementBuilder : BuilderBase
{
    private readonly IndentedTextWriter _document;
    private readonly StatesBuilder _statesBuilder;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly AfterTransitionsBuilder _afterTransitionsBuilder;
    private readonly ObservableBuilder _observableBuilder;

    public StatesManagementBuilder(IndentedTextWriter document,
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
            var state = _statesBuilder.GetState(stateName);

            var onEntryTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnEntry).ToList();
            var hasEntryTransitions = onEntryTransitions.Count > 0;


            var triggerTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnTrigger).ToList();
            var hasTriggerTransactions = triggerTransitions.Count > 0;

            var hasAfterTransitions = _afterTransitionsBuilder.HasAfterTransitions(stateName);

            var parameters = string.Join(", ", state.Parameters.Select(p => $"{p.Modifiers}{(string.IsNullOrEmpty(p.Modifiers) ? "" : " ")}{p.ParameterType} {p.Name}"));
            firstStateMethod = _document.WriteSeparatorLine(firstStateMethod);
            _document.WriteLine($"{state.Modifiers} async {state.ReturnType} {state.Name}({parameters})");
            _document.WriteLineBlockOpen();

            var onExitCall = CreateOnExitCall(state, null);

            if (hasEntryTransitions || hasTriggerTransactions || hasAfterTransitions)
            {
                _document.WriteLine($"switch ({_triggersBuilder.LastTriggerFieldName})");
                _document.WriteLineBlockOpen();
                var first = true;

                if (hasEntryTransitions || hasAfterTransitions)
                {
                    first = _document.WriteSeparatorLine(first);

                    _document.WriteLine($"case {_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName}:");
                    _document.Indent++;

                    _afterTransitionsBuilder.AddStartTimers(stateName);
                    foreach (var transitionDeclaration in onEntryTransitions)
                    {
                        _document.WriteConditionAndAction(transitionDeclaration);
                    }
                    _document.WriteLine("break;");
                    _document.Indent--;
                }

                if (hasTriggerTransactions)
                {
                    var triggersGrouped = triggerTransitions.GroupBy(trigger => trigger.Trigger);
                    foreach (var trigger in triggersGrouped)
                    {
                        first = _document.WriteSeparatorLine(first);
                        _document.WriteLine($"case {_triggersBuilder.TriggerEnumTypeName}.{trigger.Key}:");
                        _document.Indent++;
                        foreach (var transition in trigger.ToList())
                        {
                            _document.WriteConditionActionAndTransition(transition, onExitCall,
                                (document, declaration) =>
                                {
                                    document.WriteLine($"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"Trigger: {trigger.Key}\");");
                                }
                            );
                        }
                        _document.WriteLine("break;");
                        _document.Indent--;
                    }
                }

                if (hasAfterTransitions)
                {
                    _afterTransitionsBuilder.AddTimerTransitions(stateName, first, onExitCall);
                }

                _document.WriteLineBlockClose();
            }

            _document.WriteLineBlockClose();
        }
    }

    private string? CreateOnExitCall(MethodDeclaration state, string? cancellationParameter)
    {
        var onExitTransitions = state.Transitions
            .Where(transition => transition.TransitionType == TransitionType.OnExit).ToList();
        var hasExitTransactions = onExitTransitions.Count > 0;

        if (hasExitTransactions)
        {
            var onExitCall = "OnExit();";
            using var methodDocument = new SourceWriter();
            methodDocument.Indent = _document.Indent;

            methodDocument.WriteLine("void OnExit()");
            methodDocument.WriteLineBlockOpen();
            foreach (var transitionDeclaration in onExitTransitions)
            {
                methodDocument.WriteConditionAndAction(transitionDeclaration);
            }
            methodDocument.WriteLineBlockClose();
            methodDocument.WriteLineNoTabs();

            var methodSource = methodDocument.ToString()!;
            if(IsAsync(methodSource))
            {
                var parameter = cancellationParameter is not null ? $"{CommonTypeNames.CancellationToken} cancellationToken" : "";
                {

                }
                methodSource = methodSource.Replace($"async {CommonTypeNames.Task} OnExitAsync({parameter})", "");
                onExitCall = $"await OnExitAsync({cancellationParameter});";
            }

            _document.Write(methodSource);
            return onExitCall;
        }

        return null;
    }

    /// <summary>
    /// Test if the source code for a method contains any await expressions or await foreach statements, which would indicate that the method is asynchronous.
    /// </summary>
    /// <param name="code">The source code for the method.</param>
    /// <returns><see langword="true"/> if the code contains an asynchronous method, or false otherwise.</returns>
    public static bool IsAsync(string code)
    {
        if (SyntaxFactory.ParseMemberDeclaration(code) is not MethodDeclarationSyntax method || method.Body is null)
        {
            return false;
        }
        // Check if the body contains await expressions or await foreach statements
        var descendantNodes = method.Body.DescendantNodes().ToList();
        return descendantNodes.OfType<AwaitExpressionSyntax>().Any() 
            || descendantNodes.OfType<ForEachStatementSyntax>().Any(f => f.AwaitKeyword != default && !f.AwaitKeyword.IsMissing);
    }
}
