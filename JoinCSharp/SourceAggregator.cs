using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JoinCSharp;

internal class SourceAggregator(bool includeAssemblyAttributes)
{
    class ByNameUsingComparer : IEqualityComparer<UsingDirectiveSyntax>
    {
        public bool Equals(UsingDirectiveSyntax? x, UsingDirectiveSyntax? y) => Equals(x?.Name?.ToString(), y?.Name?.ToString());
        public int GetHashCode(UsingDirectiveSyntax? obj) => obj?.Name?.ToString()?.GetHashCode() ?? 0;
    }
    static readonly ByNameUsingComparer UsingComparer = new();

    List<UsingDirectiveSyntax> Usings { get; } = new();
    List<NamespaceDeclarationSyntax> Namespaces { get; } = new();
    List<MemberDeclarationSyntax> Other { get; } = new();
    List<AttributeListSyntax> AttributeLists { get; } = new();
    List<ExternAliasDirectiveSyntax> Externs { get; } = new();

    public SourceAggregator AddSource(string source)
    {
        var compilationUnit = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(source).GetRoot();
        Usings.AddRange(compilationUnit.Usings);
        Namespaces.AddRange(compilationUnit.Members.OfType<NamespaceDeclarationSyntax>());
        Other.AddRange(compilationUnit.Members.Except(Namespaces));
        if (includeAssemblyAttributes)
            AttributeLists.AddRange(compilationUnit.AttributeLists
            .Where(al => al.Target?.Identifier.Kind() == SyntaxKind.AssemblyKeyword));
        Externs.AddRange(compilationUnit.Externs);
        return this;
    }

    public string GetResult() => SyntaxFactory.CompilationUnit()
        .AddUsings(Usings.Select(u => u.WithGlobalKeyword(SyntaxFactory.Token(SyntaxKind.None))).Distinct(UsingComparer).OrderBy(u => u?.Name.ToString()).ToArray())
        .AddAttributeLists(GetConsolidatedAttributeList().ToArray())
        .AddExterns(Externs.ToArray())
        .AddMembers(GetConsolidatedNamespaces().ToArray())
        .AddMembers(Other.ToArray())
        .Format()
        .ToFullString();

    private IEnumerable<MemberDeclarationSyntax> GetConsolidatedNamespaces()
        => from @namespace in Namespaces
           let name = @namespace.Name.ToString()
           orderby name
           group @namespace by name into ns
           select SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(ns.Key))
               .AddUsings(ns.SelectMany(x => x.Usings).ToArray())
               .AddMembers(ns.SelectMany(x => x.Members).ToArray());

    private IEnumerable<AttributeListSyntax> GetConsolidatedAttributeList()
    {
        if (AttributeLists.Any())
        {
            var attributes = AttributeLists.SelectMany(al => al.Attributes).OrderBy(a => a.Name.ToString());
            yield return SyntaxFactory
                .AttributeList(new SeparatedSyntaxList<AttributeSyntax>().AddRange(attributes))
                .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)));
        }
    }
}
