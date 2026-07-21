using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class StatesBuilder : BuilderBase
{
    private readonly CSharpDocumentWriter _document;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly bool _stateIsPublic;
    private readonly Dictionary<string, MethodDeclaration> _states;
    private readonly string _stateTypeName;
    private readonly bool _hasTriggers;
    private readonly bool _stateIsAccessible;

    public StatesBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder)
    {
        _document = document;
        _classCommonBuilder = classCommonBuilder;
        _stateIsAccessible = declaration.StateAccessible;
        _stateIsPublic = declaration.StateAccessible || declaration.Observable;
        StateFieldName = _classCommonBuilder.ToPrivateName("CurrentState");
        EnterStateMethodName = _classCommonBuilder.ToPrivateName("EnterState");
        _stateTypeName = _stateIsPublic ? "State" : _classCommonBuilder.ToPrivateName("State");
        StateFullTypeName = $"{classCommonBuilder.StateMachineName}.{_stateTypeName}";
        UndefinedStateName = _classCommonBuilder.ToPrivateName("StateUndefined");
        AssertIsInitializedMethodName = _classCommonBuilder.ToPrivateName("AssertIsInitialized");

        _states = declaration.Methods.Where(method => method.IsState).ToDictionary(state => state.Name);
        _hasTriggers = declaration.Methods.Any(method => method.IsTrigger);

        InitialStateName = declaration.Methods.FirstOrDefault(method => method.IsInitial)?.Name;

        StateNames = _states.Keys.ToList();
    }

    public string StateFullTypeName { get; }

    public string StateFieldName { get; }

    public string AssertIsInitializedMethodName { get; }

    public string UndefinedStateName { get; }

    public string? InitialStateName { get; }

    public List<string> StateNames { get; }

    public string EnterStateMethodName { get; }

    public MethodDeclaration GetState(string stateName) => _states[stateName];

    public override bool IsEnabled => HasStates || _hasTriggers;

    public bool HasStates => StateNames.Count > 0;

    public override bool AddTypes()
    {
        var stateVisibility = _stateIsPublic ? "public" : "private";

        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Enumeration of the states that the state machine can be in.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine("/// <remarks>");
        _document.WriteLine($"/// The names of the members of the <see cref=\"{StateFullTypeName}\" /> enum are the names of the state methods of the <see cref=\"{_classCommonBuilder.StateMachineName}\" /> class.");
        _document.WriteLine("/// </remarks>");
        _document.WriteLine($"{stateVisibility} enum {_stateTypeName}");
        _document.WriteLineBlockOpen();
        _document.AddEnumMembers([.. _states.Keys], firstValue: 1);
        _document.WriteLineBlockClose();

        return true;
    }

    public override bool AddConstants()
    {
        _document.WriteLine($"private const {StateFullTypeName} {UndefinedStateName} = ({StateFullTypeName})0;");

        return true;
    }

    public override bool AddFields()
    {
        _document.WriteLine($"private {StateFullTypeName} {StateFieldName} = {UndefinedStateName};");

        return true;
    }

    public override bool AddPublicProperties()
    {
        return AddCurrentStateProperty();
    }

    private bool AddCurrentStateProperty()
    {
        if (!_stateIsAccessible)
        {
            return false;
        }

        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Readonly property to get the current state the state machine is in.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine("/// <value>");
        _document.WriteLine("/// The current state of the state machine.");
        _document.WriteLine("/// </value>");
        _document.WriteLine("/// <exception cref=\"global::System.InvalidOperationException\">");
        _document.WriteLine("/// the state is not initialized yet");
        _document.WriteLine("/// </exception>");
        _document.WriteLine("public State CurrentState");
        _document.WriteLineBlockOpen();
        _document.WriteLine("get");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"{AssertIsInitializedMethodName}();");
        _document.WriteLineNoTabs();
        _document.WriteLine($"return {StateFieldName};");
        _document.WriteLineBlockClose();
        _document.WriteLineBlockClose();

        return true;
    }

    public override bool AddPrivateMethods()
    {
        _document.WriteLine($"private void {AssertIsInitializedMethodName}()");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"if ({StateFieldName} == {UndefinedStateName})");
        _document.WriteLineBlockOpen();
        _document.WriteLine($"""throw new global::System.InvalidOperationException("The state machine is not initialized yet. Call 'InitializeStateMachine()' on the {_classCommonBuilder.StateMachineName} instance before using it.");""");
        _document.WriteLineBlockClose();
        _document.WriteLineBlockClose();

        return true;
    }
}
