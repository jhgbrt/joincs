using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace JoinCSharp
{
    public class SourceAggregator
    {
        class UsingComparer : IEqualityComparer<UsingDirectiveSyntax>
        {
            public bool Equals(UsingDirectiveSyntax x, UsingDirectiveSyntax y) => x.Name.ToString().Equals(y.Name.ToString());
            public int GetHashCode(UsingDirectiveSyntax obj) => obj.Name.ToString().GetHashCode();
        }

        private CSharpParseOptions _options;

        List<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();
        List<NamespaceDeclarationSyntax> Namespaces { get; } = new List<NamespaceDeclarationSyntax>();
        List<MemberDeclarationSyntax> Other { get; } = new List<MemberDeclarationSyntax>();

        public SourceAggregator(params string[] preprocessorSymbols)
        {
            _options = CSharpParseOptions.Default.WithPreprocessorSymbols(preprocessorSymbols);
        }

        public SourceAggregator AddSource(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, _options);
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            Usings.AddRange(compilationUnit.Usings);
            Namespaces.AddRange(compilationUnit.Members.OfType<NamespaceDeclarationSyntax>());
            Other.AddRange(compilationUnit.Members.Except(Namespaces));
            return this;
        }

        public string GetResult()
        {
            var namespaces = (
                from @namespace in Namespaces.WithoutTrivia()
                let name = @namespace.Name.ToString()
                orderby name
                group @namespace by name into ns
                select SyntaxFactory
                    .NamespaceDeclaration(SyntaxFactory.ParseName(ns.Key))
                    .AddMembers(ns.SelectMany(x => x.Members.WithoutTrivia()).WithoutTrivia().ToArray())
            ).ToArray();

            var cs = SyntaxFactory.CompilationUnit()
                .AddUsings(Usings.Distinct(new UsingComparer()).WithoutTrivia().ToArray())
                .AddMembers(namespaces.ToArray())
                .AddMembers(Other.ToArray())
                .NormalizeWhitespace();
            return cs.ToString();
        }
    }
}
