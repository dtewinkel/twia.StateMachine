using System.CodeDom.Compiler;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders;

internal class ObservableBuilder : BuilderBase
{
    private readonly IndentedTextWriter _document;
    private readonly StateMachineDeclaration _declaration;
    private readonly StatesBuilder _statesBuilder;
    private readonly string _stateChangedMethodName;


    public ObservableBuilder(IndentedTextWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder)
    {
        _document = document;
        _declaration = declaration;
        _statesBuilder = statesBuilder;

        _stateChangedMethodName = classCommonBuilder.ToPrivateName("StateChanged");
    }

    public override bool IsEnabled => _declaration.Observable;

    public override bool AddPrivateMethods()
    {
        _document.WriteLine($"private void {_stateChangedMethodName}(");
        _document.Indent++;
        _document.WriteLine($"{_statesBuilder.StateFullTypeName} fromState,");
        _document.WriteLine($"{_statesBuilder.StateFullTypeName} toState,");
        _document.WriteLine("string reason");
        _document.WriteLine(")");
        _document.Indent--;
        _document.WriteLineBlockOpen();
        _document.WriteLine($"var nullableFromState = fromState == {_statesBuilder.UndefinedStateName} ? ({_statesBuilder.StateFullTypeName}?)null : fromState;");
        _document.WriteLine("OnStateChanged?.Invoke(this,");
        _document.Indent++;
        _document.WriteLine($"new global::Twia.StateMachine.StateChangedEventArgs<{_statesBuilder.StateFullTypeName}>(nullableFromState, toState, reason)");
        _document.Indent--;
        _document.WriteLine(");");
        _document.WriteLineBlockClose();
        return true;
    }

    public override bool AddEvents()
    {
        _document.WriteLine("/// <inheritdoc />");
        _document.WriteLine($"public event global::System.EventHandler<global::Twia.StateMachine.StateChangedEventArgs<{_statesBuilder.StateFullTypeName}>>? OnStateChanged;");
        return true;
    }

    public void AddObserveStateChange(string newStateParameterName, string reasonParameterName)
    {
        if (IsEnabled)
        {
            _document.WriteLine($"{_stateChangedMethodName}({_statesBuilder.StateFieldName}, {newStateParameterName}, {reasonParameterName});");
            _document.WriteLineNoTabs();
        }
    }
}
