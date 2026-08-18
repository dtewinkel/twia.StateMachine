using System.CodeDom.Compiler;
using System.Globalization;
using System.Text;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator;

public class CSharpDocumentWriter(int capacity = 10000)
    : IndentedTextWriter(new StringWriter(new StringBuilder(capacity), CultureInfo.InvariantCulture))
{
    public override string ToString()
    {
        return InnerWriter.ToString() ?? "";
    }

    public void WriteLineBlockOpen()
    {
        WriteLine("{");
        Indent++;
    }

    public void WriteLineBlockClose()
    {
        Indent--;
        WriteLine("}");
    }

    public void WriteLineNoTabs()
    {
        WriteLineNoTabs("");
    }

    public bool WriteSeparatorLine(bool first)
    {
        if (!first)
        {
            WriteLineNoTabs();
        }
        return false;
    }

    public void AddEnumMembers(List<string> members, bool moreToFollow = false, int? firstValue = null)
    {
        var count = members.Count;
        var position = 1;
        foreach (var member in members)
        {
            Write($"{member}");
            if (firstValue.HasValue && position == 1)
            {
                Write($" = {firstValue.Value}");
            }
            WriteLine(position == count && !moreToFollow ? "" : ",");
            position++;
        }
    }

    public void WriteConditionActionAndTransition(TransitionDeclaration transitionDeclaration, string? onExitCall, Action<CSharpDocumentWriter, TransitionDeclaration> addBody)
    {
        var condition = transitionDeclaration.Condition;
        var hasCondition = !string.IsNullOrWhiteSpace(condition);
        var action = transitionDeclaration.Action;
        var hasAction = !string.IsNullOrWhiteSpace(action);

        if (hasCondition)
        {
            WriteLine($"if ({condition})");
            WriteLineBlockOpen();
        }

        if (!string.IsNullOrWhiteSpace(onExitCall))
        {
            WriteLine(onExitCall);
        }

        if (hasAction)
        {
            WriteLine($"{action};");
            WriteLineNoTabs();
        }

        addBody(this, transitionDeclaration);

        if (hasCondition)
        {
            WriteLineBlockClose();
        }
    }

    public void WriteMethod(string methodDeclaration, CSharpDocumentWriter methodBodyDocument)
    {
        WriteLine(methodDeclaration);
        WriteLineBlockOpen();
        foreach (var line in methodBodyDocument.ToString().Trim(methodBodyDocument.CoreNewLine).Split(methodBodyDocument.NewLine))
        {
            WriteLine(line);
        }
        WriteLineBlockClose();
    }

    public void WriteConditionAndAction(TransitionDeclaration transitionDeclaration)
    {
        var condition = transitionDeclaration.Condition;
        var hasCondition = !string.IsNullOrWhiteSpace(condition);
        var action = transitionDeclaration.Action;

        if (hasCondition)
        {
            WriteLine($"if ({condition})");
            WriteLineBlockOpen();
        }
        WriteLine($"{action};");
        if (hasCondition)
        {
            WriteLineBlockClose();
        }
    }

    public MethodReturnType GetMethodReturnType()
    {
        using var methodCode = new CSharpDocumentWriter();

        methodCode.WriteLine("public void GetReturnType()");
        methodCode.WriteLineBlockOpen();
        methodCode.WriteLine(this);
        methodCode.WriteLineBlockClose();

        return MethodReturnTypeDetector.Detect(methodCode.ToString());
    }
}