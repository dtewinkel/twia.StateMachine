using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public sealed record NamespaceDeclaration : ParentDeclaration
{
    private NamespaceDeclaration(BaseNamespaceDeclarationSyntax namespaceDeclaration) : base(namespaceDeclaration)
    {
        Name = namespaceDeclaration.Name.ToString();
        switch (namespaceDeclaration.Parent)
        {
            case NamespaceDeclarationSyntax parentNamespaceDeclaration:
                Parent = new NamespaceDeclaration(parentNamespaceDeclaration);
                break;
            case FileScopedNamespaceDeclarationSyntax parentNamespaceDeclaration:
                Parent = new NamespaceDeclaration(parentNamespaceDeclaration);
                break;
            default:
                break;
        }
    }

    public NamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclaration) : this(
        (BaseNamespaceDeclarationSyntax)namespaceDeclaration)
    {
    }

    public NamespaceDeclaration(FileScopedNamespaceDeclarationSyntax namespaceDeclaration) : this(
        (BaseNamespaceDeclarationSyntax)namespaceDeclaration)
    {
    }

    public string Name { get; }

    public NamespaceDeclaration? Parent { get; }

    public override string FullNamespaceName => Parent is null ? Name : $"{Parent.FullNamespaceName}.{Name}";

    public override string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{Name}";

    public bool Equals(NamespaceDeclaration? other)
    {
        return other is not null
               && other.Name == Name
               && ((Parent is null && other.Parent is null) || (Parent?.Equals(other.Parent) ?? false));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 29;
            hash = hash * 37 + Name.GetHashCode();
            return hash * 37 + (Parent?.GetHashCode() ?? 0);
        }
    }
}