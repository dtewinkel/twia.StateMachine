using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

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
        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Initialize the state machine before it is used.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine("/// <remarks>");
        _document.WriteLine("/// The state machine must be initialize once (and only once) before any of the other generated methods and properties can be used.");
        _document.WriteLine("/// </remarks>");
        _document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        _document.WriteLine("/// InitializeStateMachine() can only be called one in the life of a state machine");
        _document.WriteLine("/// </exception>");
        _document.WriteLine("public void InitializeStateMachine()");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"if ({_statesBuilder.StateFieldName} != {_statesBuilder.UndefinedStateName})");
        _document.WriteLineBlockOpen();
        _document.WriteLine("""throw new global::System.InvalidOperationException("'InitializeStateMachine()' can only be called once in the lifecycle of an instance.");""");
        _document.WriteLineBlockClose();
        if (initialStateName is not null)
        {
            _document.WriteLineNoTabs();
            _document.WriteLine($"// Move to initial state '{initialStateName}'.");
            _document.WriteLine($"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{initialStateName}, \"Initial\");");
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
        _document.WriteLine($"private void {_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName} {stateParameterName}, string {reasonParameterName})");
        _document.WriteLineBlockOpen();
        _afterTransitionsBuilder.AddClearTimers();
        _observableBuilder.AddObserveStateChange(stateParameterName, reasonParameterName);
        _document.WriteLine($"{_statesBuilder.StateFieldName} = {stateParameterName};");
        _document.WriteLine($"{_triggersBuilder.InvokeTriggerMethodName}({_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName});");
        _document.WriteLineBlockClose();
    }

    private void AddInvokeTriggerMethod()
    {
        _document.WriteLine($"private void {_triggersBuilder.InvokeTriggerMethodName}({_triggersBuilder.TriggerEnumTypeName} trigger)");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{_triggersBuilder.LastTriggerFieldName} = trigger;");
        if (_statesBuilder.HasStates)
        {
            _document.WriteLineNoTabs();
            _document.WriteLine($"switch ({_statesBuilder.StateFieldName})");
            _document.WriteLineBlockOpen();
            var first = true;
            foreach (var stateName in _statesBuilder.StateNames)
            {
                first = _document.WriteSeparatorLine(first);
                _document.WriteLine($"case {_statesBuilder.StateFullTypeName}.{stateName}:");
                _document.Indent++;
                _document.WriteLine($"{stateName}();");
                _document.WriteLine("break;");
                _document.Indent--;
            }

            _document.WriteLineBlockClose();
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

            var onExitTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnExit).ToList();
            var hasExitTransactions = onExitTransitions.Count > 0;

            var triggerTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.OnTrigger).ToList();
            var hasTriggerTransactions = triggerTransitions.Count > 0;

            var triggerlessTransitions = state.Transitions
                .Where(transition => transition.TransitionType == TransitionType.Triggerless).ToList();
            var hasTriggerlessTransitions = triggerlessTransitions.Count > 0;

            var hasAfterTransitions = _afterTransitionsBuilder.HasAfterTransitions(stateName);

            firstStateMethod = _document.WriteSeparatorLine(firstStateMethod);
            _document.WriteLine($"{state.Modifiers} {state.ReturnType} {state.Name}()");
            _document.WriteLineBlockOpen();

            var onExitCall = hasExitTransactions ? $"OnExit{state.Name}();" : null;

            if (hasExitTransactions)
            {
                _document.WriteLine($"void OnExit{state.Name}()");
                _document.WriteLineBlockOpen();
                foreach (var transitionDeclaration in onExitTransitions)
                {
                    _document.WriteConditionAndAction(transitionDeclaration);
                }
                _document.WriteLineBlockClose();
                _document.WriteLineNoTabs();
            }

            if (hasEntryTransitions || hasTriggerTransactions || hasAfterTransitions || hasTriggerlessTransitions)
            {
                _document.WriteLine($"switch ({_triggersBuilder.LastTriggerFieldName})");
                _document.WriteLineBlockOpen();
                var first = true;

                if (hasEntryTransitions || hasAfterTransitions || hasTriggerlessTransitions)
                {
                    first = _document.WriteSeparatorLine(first);

                    _document.WriteLine($"case {_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.EntryTriggerName}:");
                    _document.Indent++;

                    _afterTransitionsBuilder.AddStartTimers(stateName);
                    foreach (var transitionDeclaration in onEntryTransitions)
                    {
                        _document.WriteConditionAndAction(transitionDeclaration);
                    }

                    if (hasTriggerlessTransitions)
                    {
                        foreach (var transition in triggerlessTransitions)
                        {
                            _document.WriteConditionActionAndTransition(transition, onExitCall,
                                (document, declaration) =>
                                {
                                    document.WriteLine($"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"Triggerless\");");
                                }
                            );
                        }
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
}
