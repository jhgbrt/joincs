using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace JoinCSharp
{
    internal class SourceAggregator
    {
        private readonly bool _includeAssemblyAttributes;
        public SourceAggregator(bool includeAssemblyAttributes) => _includeAssemblyAttributes = includeAssemblyAttributes;

        class ByNameUsingComparer : IEqualityComparer<UsingDirectiveSyntax>
        {
            public bool Equals(UsingDirectiveSyntax? x, UsingDirectiveSyntax? y) => Equals(x?.Name.ToString(), y?.Name.ToString());
            public int GetHashCode(UsingDirectiveSyntax obj) => obj.Name.ToString().GetHashCode();
        }
        static ByNameUsingComparer UsingComparer = new ();

        List<UsingDirectiveSyntax> Usings { get; } = new();
        List<NamespaceDeclarationSyntax> Namespaces { get; } = new();
        List<MemberDeclarationSyntax> Other { get; } = new();
        List<AttributeListSyntax> AttributeLists { get; } = new();
        List<ExternAliasDirectiveSyntax> Externs { get; } = new();

        public SourceAggregator AddSource(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            Usings.AddRange(compilationUnit.Usings);
            Namespaces.AddRange(compilationUnit.Members.OfType<NamespaceDeclarationSyntax>());
            Other.AddRange(compilationUnit.Members.Except(Namespaces));
            if (_includeAssemblyAttributes)
                AttributeLists.AddRange(compilationUnit.AttributeLists.Where(al => al.Target?.Identifier.Kind() == SyntaxKind.AssemblyKeyword));
            Externs.AddRange(compilationUnit.Externs);
            return this;
        }
        
        public string GetResult()
        {
            var namespaces = (
                    from @namespace in Namespaces
                    let name = @namespace.Name.ToString()
                    orderby name
                    group @namespace by name
                    into ns
                    select SyntaxFactory
                        .NamespaceDeclaration(SyntaxFactory.ParseName(ns.Key))
                        .AddUsings(ns.SelectMany(x => x.Usings).ToArray())
                        .AddMembers(ns.SelectMany(x => x.Members).ToArray())
                )
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            var attributeList = AttributeLists.ToArray();

            if (attributeList.Any())
            {
                var attributes = 
                        from al in AttributeLists
                        from attribute in al.Attributes
                        orderby attribute.Name.ToString()
                        select attribute;

                attributeList = new[]
                {
                    SyntaxFactory.AttributeList(
                        new SeparatedSyntaxList<AttributeSyntax>().AddRange(attributes)
                    )
                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(
                        SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)
                        )
                    ) 
                };
            }


            var cs = SyntaxFactory.CompilationUnit()
                .AddUsings(Usings.Distinct(UsingComparer).OrderBy(u => u.Name.ToString()).ToArray())
                .AddAttributeLists(attributeList)
                .AddExterns(Externs.ToArray())
                .AddMembers(namespaces.ToArray())
                .AddMembers(Other.ToArray())
                .NormalizeWhitespace();

            return cs.ToFullString();
        }
    }
}
