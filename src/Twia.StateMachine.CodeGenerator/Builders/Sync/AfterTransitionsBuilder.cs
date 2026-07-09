using System.CodeDom.Compiler;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class AfterTransitionsBuilder : BuilderBase, ITriggersProvider
{
    private readonly IndentedTextWriter _document;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly StatesBuilder _statesBuilder;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly Dictionary<string, List<TransitionDeclaration>> _transitions = [];
    private readonly string _startTimerMethodName;
    private readonly string _timersBackingFieldName;

    public AfterTransitionsBuilder(IndentedTextWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder, TriggersBuilder triggersBuilder)
    {
        _document = document;
        _classCommonBuilder = classCommonBuilder;
        _statesBuilder = statesBuilder;
        _triggersBuilder = triggersBuilder;

        var states = declaration.Methods.Where(method => method.IsState).ToArray();

        foreach (var state in states)
        {
            var afterTriggers = state.Transitions.Where(transition => transition.TransitionType == TransitionType.AfterDelay).ToList();
            if (afterTriggers.Count > 0)
            {
                _transitions.Add(state.Name, afterTriggers);
            }
        }

        _timersBackingFieldName = _classCommonBuilder.ToPrivateName("Timers");
        _startTimerMethodName = _classCommonBuilder.ToPrivateName("StartTimer");
    }

    public string[] GetTriggerNames() => [.. _transitions.Values
        .SelectMany(transitions => transitions
            .Select(transition => ToFullAfterTriggerName(transition.Name)))];

    public override bool IsEnabled => _transitions.Count > 0;

    private string ToFullAfterTriggerName(string name) => _classCommonBuilder.ToPrivateName(name);

    public override bool AddFields()
    {
        _document.WriteLine($"private global::System.Collections.Generic.List<global::System.Threading.Timer> {_timersBackingFieldName} = [];");
        return true;
    }
    public override bool AddPrivateMethods()
    {
        var timerCallback = _classCommonBuilder.ToPrivateName("TimerCallback");

        _document.WriteLine($"private void {_startTimerMethodName}(string period, {_triggersBuilder.TriggerEnumTypeName} trigger)");
        _document.WriteLineBlockOpen();
        _document.WriteLine("if (global::System.TimeSpan.TryParse(period, out var timeSpan))");
        _document.WriteLineBlockOpen();
        _document.WriteLine("var milliSeconds = global::System.Convert.ToInt32(timeSpan.TotalMilliseconds);");
        _document.WriteLine($"var timer = new global::System.Threading.Timer({timerCallback}, trigger, milliSeconds, global::System.Threading.Timeout.Infinite);");
        _document.WriteLine($"{_timersBackingFieldName}.Add(timer);");
        _document.WriteLineBlockClose();
        _document.WriteLineBlockClose();
        _document.WriteLineNoTabs();
        _document.WriteLine($"private void {timerCallback}(object? state)");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{_triggersBuilder.InvokeTriggerMethodName}(({_triggersBuilder.TriggerEnumTypeName})state!);");
        _document.WriteLineBlockClose();
        return true;
    }

    public void AddClearTimers()
    {
        if (IsEnabled)
        {
            _document.WriteLine($"if ({_timersBackingFieldName}.Count > 0)");
            _document.WriteLineBlockOpen();
            _document.WriteLine($"foreach (var timer in {_timersBackingFieldName})");
            _document.WriteLineBlockOpen();
            _document.WriteLine("timer.Dispose();");
            _document.WriteLineBlockClose();
            _document.WriteLine($"{_timersBackingFieldName} = [];");
            _document.WriteLineBlockClose();
            _document.WriteLineNoTabs();
        }
    }

    public void AddStartTimers(string stateName)
    {
        if (_transitions.TryGetValue(stateName, out var transitions))
        {
            foreach (var transition in transitions)
            {
                _document.WriteLine(
                    $"{_startTimerMethodName}(\"{transition.Trigger}\", {_triggersBuilder.TriggerEnumTypeName}.{ToFullAfterTriggerName(transition.Name)});");
            }
        }
    }

    public void AddTimerTransitions(string stateName, bool first, string? onExitCall)
    {
        if (_transitions.TryGetValue(stateName, out var transitions))
        {
            foreach (var transition in transitions)
            {
                first = _document.WriteSeparatorLine(first);
                _document.WriteLine(
                    $"case {_triggersBuilder.TriggerEnumTypeName}.{ToFullAfterTriggerName(transition.Name)}:");
                _document.Indent++;
                _document.WriteConditionActionAndTransition(transition, onExitCall,
                    (document, declaration) =>
                    {
                        document.WriteLine(
                            $"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"After: {declaration.Trigger}\");");
                    }
                );

                _document.WriteLine("break;");
                _document.Indent--;
            }
        }
    }

    public bool HasAfterTransitions(string stateName)
    {
        return _transitions.ContainsKey(stateName);
    }
}