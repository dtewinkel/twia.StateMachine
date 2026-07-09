using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public abstract partial record Declaration([property: IgnoreEquality] CSharpSyntaxNode Node);