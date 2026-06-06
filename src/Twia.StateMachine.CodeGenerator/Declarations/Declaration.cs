using Generator.Equals;
using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable(IgnoreInheritedMembers = true)]
public abstract partial record Declaration([property: IgnoreEquality] CSharpSyntaxNode Node);