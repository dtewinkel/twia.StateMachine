using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Twia.StateMachine.CodeGenerator.Builders;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator;

public class StateMachineSourceBuilder
{
    public void AddSource(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        try
        {
            using var writer = new StringWriter(new StringBuilder(10000), CultureInfo.InvariantCulture);
            using var document = new IndentedTextWriter(writer);

            var classCommonBuilder = new ClassCommonBuilder(document, declaration);
            var statesBuilder = new StatesBuilder(document, declaration, classCommonBuilder);
            var triggersBuilder = new TriggersBuilder(document, declaration, classCommonBuilder, statesBuilder);
            var afterTransitionsBuilder = new AfterTransitionsBuilder(document, declaration, classCommonBuilder, statesBuilder, triggersBuilder);

            var triggerProviders = new List<ITriggersProvider> { triggersBuilder, afterTransitionsBuilder }
                .Where(provider => provider.IsEnabled)
                .ToList();

            var triggersEnumBuilder = new TriggersEnumBuilder(document, triggersBuilder, triggerProviders);
            var observableBuilder = new ObservableBuilder(document, declaration, classCommonBuilder, statesBuilder);
            var statesManagementBuilder = new StatesManagementBuilder(document, statesBuilder, triggersBuilder, afterTransitionsBuilder, observableBuilder);

            var builders = new List<BuilderBase>
            {
                statesBuilder, statesManagementBuilder, triggersBuilder, triggersEnumBuilder, afterTransitionsBuilder, observableBuilder
            }
                .Where(builder => builder.IsEnabled)
                .ToList();

            classCommonBuilder.StartClass();

            var codeAdded = GenerateAll(builders, document, false, sourceBuilder => sourceBuilder.AddTypes());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddConstants());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddFields());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddEvents());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddPublicProperties());
            codeAdded = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddPublicMethods());
            _ = GenerateAll(builders, document, codeAdded, sourceBuilder => sourceBuilder.AddPrivateMethods());

            classCommonBuilder.EndClass();

            Debug.Assert(document.Indent == 0);

            var hintName = $"{declaration.HintNameForSource}_StateMachine.g.cs";
            context.AddSource(hintName, SourceText.From(writer.ToString(), Encoding.UTF8));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    private static List<T> GetAll<T>(List<BuilderBase> builders, Func<BuilderBase, T[]> builderFunction)
        => [ .. builders.Where(builder => builder.IsEnabled).SelectMany(builderFunction) ];

    private static bool GenerateAll(List<BuilderBase> builders, IndentedTextWriter document, bool codeAdded, Func<BuilderBase, bool> builderAction)
    {
        foreach (var sourceBuilder in builders.Where(sourceBuilder => sourceBuilder.IsEnabled))
        {
            if (codeAdded)
            {
                document.WriteLineNoTabs();
            }

            codeAdded = builderAction(sourceBuilder);
        }

        return codeAdded;
    }
}