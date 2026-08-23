using System.CodeDom.Compiler;
using System.Xml;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class AfterTransitionsBuilder : BuilderBase, ITriggersProvider
{
    private readonly CSharpDocumentWriter _document;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly StatesBuilder _statesBuilder;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly Dictionary<string, List<TransitionDeclaration>> _transitions = [];
    private readonly string _startTimerMethodName;
    private readonly string _timersBackingFieldName;

    public AfterTransitionsBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder, TriggersBuilder triggersBuilder)
    {
        _document = document;
        _classCommonBuilder = classCommonBuilder;
        _statesBuilder = statesBuilder;
        _triggersBuilder = triggersBuilder;

        var states = declaration.Methods.Where(method => method.IsState).ToArray();

        foreach (var state in states)
        {
            var afterTriggers = state.Transitions
                .Where(transition => transition.TransitionType is TransitionType.AfterDelay or TransitionType.InternalAfter or TransitionType.InternalAfterEvery)
                .ToList();
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

        _document.WriteLine($"private void {_startTimerMethodName}(string delay, string? periode, {_triggersBuilder.TriggerEnumTypeName} trigger)");
        _document.WriteLineBlockOpen();
        _document.WriteLine("var delayTimeSpan = global::System.TimeSpan.Parse(delay);");
        _document.WriteLine("var periodeTimeSpan = string.IsNullOrWhiteSpace(periode) ? global::System.Threading.Timeout.InfiniteTimeSpan : global::System.TimeSpan.Parse(periode);");
        _document.WriteLine($"var timer = new global::System.Threading.Timer({timerCallback}, trigger, delayTimeSpan, periodeTimeSpan);");
        _document.WriteLine($"{_timersBackingFieldName}.Add(timer);");
        _document.WriteLineBlockClose();
        _document.WriteLineNoTabs();
        _document.WriteLine($"private void {timerCallback}(object? trigger)");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{_triggersBuilder.InvokeTriggerMethodName}(({_triggersBuilder.TriggerEnumTypeName})trigger!);");
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
                var timeSpan = ParsePeriod(transition.Trigger);

                switch (transition.TransitionType)
                {
                    case TransitionType.AfterDelay:
                    case TransitionType.InternalAfter:
                        _document.WriteLine(
                            $"{_startTimerMethodName}(\"{timeSpan}\", null, {_triggersBuilder.TriggerEnumTypeName}.{ToFullAfterTriggerName(transition.Name)});");
                        break;

                    case TransitionType.InternalAfterEvery:
                        var initialDelay = ((InternalAfterEveryDeclaration)transition).InitialDelay;
                        var initialTimeSpan = string.IsNullOrWhiteSpace(initialDelay) ? timeSpan.ToString() : ParsePeriod(initialDelay).ToString();
                        _document.WriteLine(
                            $"{_startTimerMethodName}(\"{initialTimeSpan}\", \"{timeSpan}\", {_triggersBuilder.TriggerEnumTypeName}.{ToFullAfterTriggerName(transition.Name)});");
                        break;
                }
            }
        }
    }


    private static TimeSpan ParsePeriod(string trigger)
    {
        if (TimeSpan.TryParse(trigger, out var timeSpan))
        {
            return timeSpan;
        }

        try
        {
            return XmlConvert.ToTimeSpan(trigger);
        }
        catch (FormatException)
        {
            return TimeSpan.Zero;
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
                switch (transition.TransitionType)
                {
                    case TransitionType.AfterDelay:
                        _document.WriteConditionActionAndTransition(transition, onExitCall,
                            (document, declaration) =>
                            {
                                document.WriteLine(
                                    $"{_statesBuilder.EnterStateMethodName}({_statesBuilder.StateFullTypeName}.{declaration.TargetState}, \"After: {declaration.Trigger}\");");
                            }
                        );
                        break;
                    case TransitionType.InternalAfter:
                    case TransitionType.InternalAfterEvery:
                        _document.WriteConditionAndAction(transition);
                        break;
                }
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