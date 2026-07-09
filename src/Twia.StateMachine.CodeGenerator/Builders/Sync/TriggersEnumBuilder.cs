using System.CodeDom.Compiler;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class TriggersEnumBuilder : BuilderBase
{
    private readonly IndentedTextWriter _document;
    private readonly TriggersBuilder _triggersBuilder;
    private readonly List<string> _triggers;

    public TriggersEnumBuilder(IndentedTextWriter document, TriggersBuilder triggersBuilder, List<ITriggersProvider> triggersProviders)
    {
        _document = document;
        _triggersBuilder = triggersBuilder;

        _triggers = [ .. triggersProviders.Where(provider => provider.IsEnabled).SelectMany(provider => provider.GetTriggerNames()) ];
    }

    public override bool IsEnabled => _triggers.Count > 0;

    public override bool AddTypes()
    {
        _document.WriteLine($"private enum {_triggersBuilder.TriggerEnumTypeName}");
        _document.WriteLineBlockOpen();
        _document.AddEnumMembers([_triggersBuilder.UndefinedTrigger], firstValue: 0, moreToFollow: true);
        _document.AddEnumMembers(_triggers);
        _document.WriteLineBlockClose();

        return true;
    }

    public override bool AddFields()
    {
        _document.WriteLine($"private {_triggersBuilder.TriggerEnumTypeName} {_triggersBuilder.LastTriggerFieldName} = {_triggersBuilder.TriggerEnumTypeName}.{_triggersBuilder.UndefinedTrigger};");

        return true;
    }
}