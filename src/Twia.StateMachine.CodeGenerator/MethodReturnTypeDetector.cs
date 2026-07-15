using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator;

public enum MethodReturnType
{
    ReturnsVoid,
    ReturnTask,
    ReturnOther,
    MixedReturn
}

public static class MethodReturnTypeDetector
{
    public static MethodReturnType Detect(string code)
    {
        var memberDeclaration = SyntaxFactory.ParseMemberDeclaration(code);
        if (memberDeclaration is not MethodDeclarationSyntax methodDeclaration)
        {
            throw new ArgumentException("Code must parse to a method declaration.", nameof(code));
        }

        return Detect(methodDeclaration);
    }

    internal static MethodReturnType Detect(MethodDeclarationSyntax methodDeclaration)
    {
        var collector = new ReturnInfoCollector();

        if (methodDeclaration.Body is not null)
        {
            collector.Visit(methodDeclaration.Body);
        }

        if (methodDeclaration.ExpressionBody is not null)
        {
            collector.Visit(methodDeclaration.ExpressionBody.Expression);
            collector.AddValueReturn(methodDeclaration.ExpressionBody.Expression);
        }

        return collector.GetReturnType();
    }

    private static MethodReturnType ClassifyExpression(ExpressionSyntax expression)
    {
        expression = Unwrap(expression);

        return expression switch
        {
            AwaitExpressionSyntax => MethodReturnType.ReturnOther,
            ConditionalExpressionSyntax conditionalExpression => Combine([
                ClassifyExpression(conditionalExpression.WhenTrue),
                ClassifyExpression(conditionalExpression.WhenFalse)
            ]),
            SwitchExpressionSyntax switchExpression => Combine(switchExpression.Arms.Select(arm => ClassifyExpression(arm.Expression))),
            _ when IsTaskExpression(expression) => MethodReturnType.ReturnTask,
            _ => MethodReturnType.ReturnOther
        };
    }

    private static MethodReturnType Combine(IEnumerable<MethodReturnType> returnTypes)
    {
        var distinctReturnTypes = returnTypes.Distinct().ToArray();
        return distinctReturnTypes.Length switch
        {
            0 => MethodReturnType.ReturnOther,
            1 => distinctReturnTypes[0],
            _ => MethodReturnType.MixedReturn
        };
    }

    private static bool IsTaskExpression(ExpressionSyntax expression)
    {
        expression = Unwrap(expression);

        return expression switch
        {
            ObjectCreationExpressionSyntax objectCreation => IsTaskType(objectCreation.Type),
            DefaultExpressionSyntax defaultExpression => IsTaskType(defaultExpression.Type),
            CastExpressionSyntax castExpression => IsTaskType(castExpression.Type),
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText == "Task",
            InvocationExpressionSyntax invocationExpression => IsTaskExpression(invocationExpression.Expression),
            MemberAccessExpressionSyntax memberAccessExpression => IsTaskExpression(memberAccessExpression.Expression)
                                                                  || IsTaskName(memberAccessExpression.Name),
            ConditionalAccessExpressionSyntax conditionalAccessExpression => IsTaskExpression(conditionalAccessExpression.Expression),
            _ => false
        };
    }

    private static bool IsTaskType(TypeSyntax typeSyntax)
    {
        return UnwrapType(typeSyntax) switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText == "Task",
            GenericNameSyntax genericName => genericName.Identifier.ValueText == "Task",
            QualifiedNameSyntax qualifiedName => IsTaskName(qualifiedName.Right),
            AliasQualifiedNameSyntax aliasQualifiedName => IsTaskName(aliasQualifiedName.Name),
            NullableTypeSyntax nullableType => IsTaskType(nullableType.ElementType),
            _ => false
        };
    }

    private static bool IsTaskName(SimpleNameSyntax simpleName)
    {
        return simpleName.Identifier.ValueText == "Task";
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            expression = parenthesizedExpression.Expression;
        }

        return expression;
    }

    private static TypeSyntax UnwrapType(TypeSyntax typeSyntax)
    {
        while (typeSyntax is RefTypeSyntax refTypeSyntax)
        {
            typeSyntax = refTypeSyntax.Type;
        }

        return typeSyntax;
    }

    private sealed class ReturnInfoCollector : CSharpSyntaxWalker
    {
        private bool _containsAwait;
        private bool _hasBareReturn;
        private bool _hasTaskReturn;
        private bool _hasOtherReturn;

        public void AddValueReturn(ExpressionSyntax expression)
        {
            switch (ClassifyExpression(expression))
            {
                case MethodReturnType.ReturnTask:
                    _hasTaskReturn = true;
                    break;
                case MethodReturnType.MixedReturn:
                    _hasTaskReturn = true;
                    _hasOtherReturn = true;
                    break;
                default:
                    _hasOtherReturn = true;
                    break;
            }
        }

        public MethodReturnType GetReturnType()
        {
            if (_containsAwait)
            {
                return _hasBareReturn && (_hasTaskReturn || _hasOtherReturn)
                    ? MethodReturnType.MixedReturn
                    : MethodReturnType.ReturnTask;
            }

            if (_hasBareReturn && (_hasTaskReturn || _hasOtherReturn))
            {
                return MethodReturnType.MixedReturn;
            }

            if (_hasTaskReturn && _hasOtherReturn)
            {
                return MethodReturnType.MixedReturn;
            }

            if (_hasTaskReturn)
            {
                return MethodReturnType.ReturnTask;
            }

            if (_hasOtherReturn)
            {
                return MethodReturnType.ReturnOther;
            }

            return MethodReturnType.ReturnsVoid;
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            _containsAwait = true;
            base.VisitAwaitExpression(node);
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            if (node.Expression is null)
            {
                _hasBareReturn = true;
            }
            else
            {
                AddValueReturn(node.Expression);
            }

            base.VisitReturnStatement(node);
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
        }
    }
}
