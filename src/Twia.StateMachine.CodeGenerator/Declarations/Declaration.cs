using Microsoft.CodeAnalysis.CSharp;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public abstract record Declaration(CSharpSyntaxNode Node);