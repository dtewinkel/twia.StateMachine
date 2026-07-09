using System.CodeDom.Compiler;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Async;

public class TriggersBuilder : BuilderBase, ITriggersProvider
{
    private readonly IndentedTextWriter _document;
    private readonly StatesBuilder _statesBuilder;

    private readonly MethodDeclaration[] _triggerMethods;

    public TriggersBuilder(IndentedTextWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder)
    {
        _document = document;
        _statesBuilder = statesBuilder;

        _triggerMethods = [.. declaration.Methods.Where(method => method.IsTrigger)];

        UndefinedTrigger = classCommonBuilder.ToPrivateName("Undefined");
        TriggerEnumTypeName = classCommonBuilder.ToPrivateName("Trigger");
        LastTriggerFieldName = classCommonBuilder.ToPrivateName("LastTrigger");

        EntryTriggerName = classCommonBuilder.ToPrivateName("Entry");

        InvokeTriggerMethodName = classCommonBuilder.ToPrivateName("InvokeTriggerAsync");
    }

    public string UndefinedTrigger { get; }

    public string TriggerEnumTypeName { get; }

    public string EntryTriggerName { get; }

    public string InvokeTriggerMethodName { get; }

    public string LastTriggerFieldName { get; }

    public override bool IsEnabled => _triggerMethods.Length > 0 || _statesBuilder.HasStates;

    public string[] GetTriggerNames() => [ .. _triggerMethods.Select(trigger => trigger.Name), EntryTriggerName ];

    public override bool AddPublicMethods()
    {
        if (_triggerMethods.Length > 0)
        {
            AddTriggerMethods();
            return true;
        }

        return false;
    }

    private void AddTriggerMethods()
    {
        var first = true;
        foreach (var trigger in _triggerMethods)
        {
            first = _document.WriteSeparatorLine(first);

            var parameters = string.Join(", ", trigger.Parameters.Select(p => $"{p.Modifiers ?? ""}{(string.IsNullOrEmpty(p.Modifiers) ? "" : " ")}{p.ParameterType} {p.Name}"));
            var cancellationTokenParamName = trigger.CancellationTokenParameterName;
            var withCancellationToken = cancellationTokenParamName != null ? $", {cancellationTokenParamName}" : "";

            _document.WriteLine($"{trigger.Modifiers} async {trigger.ReturnType} {trigger.Name}({parameters})");
            _document.WriteLineBlockOpen();
            _document.WriteLine($"{_statesBuilder.AssertIsInitializedMethodName}();");
            _document.WriteLineNoTabs();
            _document.WriteLine($"await {InvokeTriggerMethodName}({TriggerEnumTypeName}.{trigger.Name}{withCancellationToken});");
            _document.WriteLineBlockClose();
        }
    }
}