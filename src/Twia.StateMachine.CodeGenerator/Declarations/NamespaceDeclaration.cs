using Generator.Equals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twia.StateMachine.CodeGenerator.Declarations;

[Equatable]
public sealed partial record NamespaceDeclaration : ParentDeclaration
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

    [IgnoreEquality]
    public override string FullNamespaceName => Parent is null ? Name : $"{Parent.FullNamespaceName}.{Name}";

    [IgnoreEquality]
    public override string HintNameForSource => $"{(Parent is not null ? $"{Parent.HintNameForSource}." : "")}{Name}";
}