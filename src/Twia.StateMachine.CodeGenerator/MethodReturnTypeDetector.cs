using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator;

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

    private static MethodReturnType Detect(MethodDeclarationSyntax methodDeclaration)
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
            AwaitExpressionSyntax => MethodReturnType.Other,
            ConditionalExpressionSyntax conditionalExpression => Combine([
                ClassifyExpression(conditionalExpression.WhenTrue),
                ClassifyExpression(conditionalExpression.WhenFalse)
            ]),
            SwitchExpressionSyntax switchExpression => Combine(switchExpression.Arms.Select(arm => ClassifyExpression(arm.Expression))),
            _ when IsTaskExpression(expression) => MethodReturnType.Task,
            _ => MethodReturnType.Other
        };
    }

    private static MethodReturnType Combine(IEnumerable<MethodReturnType> returnTypes)
    {
        var distinctReturnTypes = returnTypes.Distinct().ToArray();
        return distinctReturnTypes.Length switch
        {
            0 => MethodReturnType.Other,
            1 => distinctReturnTypes[0],
            _ => MethodReturnType.Mixed
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
            InvocationExpressionSyntax invocationExpression => IsTaskInvocation(invocationExpression),
            MemberAccessExpressionSyntax memberAccessExpression => IsTaskExpression(memberAccessExpression.Expression)
                                                                  || IsTaskName(memberAccessExpression.Name),
            ConditionalAccessExpressionSyntax conditionalAccessExpression => IsTaskExpression(conditionalAccessExpression.Expression),
            _ => false
        };
    }

    private static bool IsTaskInvocation(InvocationExpressionSyntax invocationExpression)
    {
        // Check if the method being invoked has generic type arguments
        // If it does (e.g., FromException<int>), it returns Task<T>, not Task
        if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name is GenericNameSyntax)
        {
            return false; // Generic method invocations return Task<T>, not plain Task
        }

        return IsTaskExpression(invocationExpression.Expression);
    }

    private static bool IsTaskType(TypeSyntax typeSyntax)
    {
        return UnwrapType(typeSyntax) switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText == "Task",
            GenericNameSyntax genericName => false, // Task<T> is not plain Task
            QualifiedNameSyntax qualifiedName => IsTaskName(qualifiedName.Right),
            AliasQualifiedNameSyntax aliasQualifiedName => IsTaskName(aliasQualifiedName.Name),
            NullableTypeSyntax nullableType => IsTaskType(nullableType.ElementType),
            _ => false
        };
    }

    private static bool IsTaskName(SimpleNameSyntax simpleName)
    {
        return simpleName is IdentifierNameSyntax identifierName 
            && identifierName.Identifier.ValueText == "Task";
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
                case MethodReturnType.Task:
                    _hasTaskReturn = true;
                    break;

                case MethodReturnType.Mixed:
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
                    ? MethodReturnType.Mixed
                    : MethodReturnType.AsyncTask;
            }

            if (_hasBareReturn && (_hasTaskReturn || _hasOtherReturn))
            {
                return MethodReturnType.Mixed;
            }

            if (_hasTaskReturn && _hasOtherReturn)
            {
                return MethodReturnType.Mixed;
            }

            if (_hasTaskReturn)
            {
                return MethodReturnType.Task;
            }

            if (_hasOtherReturn)
            {
                return MethodReturnType.Other;
            }

            return MethodReturnType.Void;
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            _containsAwait = true;
            base.VisitAwaitExpression(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (node.AwaitKeyword.Kind() != SyntaxKind.None)
            {
                _containsAwait = true;
            }
            base.VisitForEachStatement(node);
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


