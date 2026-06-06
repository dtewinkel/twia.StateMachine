using Generator.Equals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable(IgnoreInheritedMembers = true)]
public partial record ClassDeclaration : Declaration
{
    public ClassDeclaration(ClassDeclarationSyntax node) : base(node)
    {
        Modifiers = node.Modifiers.ToString();
        Name = node.Identifier.Text + node.TypeParameterList?.ToFullString();
        IsPartial = node.Modifiers.Any(SyntaxKind.PartialKeyword);
        switch (node.Parent)
        {
            case ClassDeclarationSyntax classDeclaration:
                Parent = new ContainingClassDeclaration(classDeclaration);
                break;
            case NamespaceDeclarationSyntax namespaceDeclaration:
                Parent = new NamespaceDeclaration(namespaceDeclaration);
                break;
            case FileScopedNamespaceDeclarationSyntax namespaceDeclaration:
                Parent = new NamespaceDeclaration(namespaceDeclaration);
                break;
            default:
                return;
        }
    }
    
    public string Modifiers { get; }
    public string Name { get; }
    public bool IsPartial { get; }
    public ParentDeclaration? Parent { get; }

    [IgnoreEquality]
    public string? FullNamespaceName => Parent?.FullNamespaceName;

    [IgnoreEquality]
    public string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{Name}";
}