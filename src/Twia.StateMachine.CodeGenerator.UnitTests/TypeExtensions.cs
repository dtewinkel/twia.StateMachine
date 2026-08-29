using System;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

public static class TypeExtensions
{
    public static string GetNamespacePrefix(this Type type)
    {
        return string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"{type.Namespace}.";
    }
}
