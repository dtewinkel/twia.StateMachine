using System.CodeDom.Compiler;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class TriggersBuilder : BuilderBase, ITriggersProvider
{
    private readonly CSharpDocumentWriter _document;
    private readonly StatesBuilder _statesBuilder;

    private readonly MethodDeclaration[] _triggerMethods;

    public TriggersBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder)
    {
        _document = document;
        _statesBuilder = statesBuilder;

        _triggerMethods = [.. declaration.Methods.Where(method => method.IsTrigger)];

        UndefinedTrigger = classCommonBuilder.ToPrivateName("Undefined");
        TriggerEnumTypeName = classCommonBuilder.ToPrivateName("Trigger");
        LastTriggerFieldName = classCommonBuilder.ToPrivateName("LastTrigger");

        EntryTriggerName = classCommonBuilder.ToPrivateName("Entry");

        InvokeTriggerMethodName = classCommonBuilder.ToPrivateName("InvokeTrigger");
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
            _document.WriteLine($"{trigger.Modifiers} {trigger.ReturnType} {trigger.Name}()");
            _document.WriteLineBlockOpen();
            _document.WriteLine($"{_statesBuilder.AssertIsInitializedMethodName}();");
            _document.WriteLineNoTabs();
            _document.WriteLine($"{InvokeTriggerMethodName}({TriggerEnumTypeName}.{trigger.Name});");
            _document.WriteLineBlockClose();
        }
    }
}