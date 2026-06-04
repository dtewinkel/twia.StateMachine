using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public record ClassDeclaration : Declaration
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

    public string? FullNamespaceName => Parent?.FullNamespaceName;

    public string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{Name}";

    public virtual bool Equals(ClassDeclaration? other)
    {
        if (other is null)
        {
            return false;
        }

        return Modifiers == other.Modifiers
               && Name == other.Name
               && IsPartial == other.IsPartial
               && (ReferenceEquals(Parent, other.Parent) || (Parent?.Equals(other.Parent) ?? false));
    }

    public override int GetHashCode() 
        => HashCode.Combine(Name, Modifiers, IsPartial, Parent);
}