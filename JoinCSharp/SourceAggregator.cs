using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace JoinCSharp
{
    public class SourceAggregator
    {
        private bool _ignoreAssemblyAttributeLists;
        public SourceAggregator(bool ignoreAssemblyAttributeLists)
        {
            _ignoreAssemblyAttributeLists = ignoreAssemblyAttributeLists;
        }

        class UsingComparer : IEqualityComparer<UsingDirectiveSyntax>
        {
            public bool Equals(UsingDirectiveSyntax x, UsingDirectiveSyntax y) => x.Name.ToString().Equals(y.Name.ToString());
            public int GetHashCode(UsingDirectiveSyntax obj) => obj.Name.ToString().GetHashCode();
        }

        List<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();
        List<NamespaceDeclarationSyntax> Namespaces { get; } = new List<NamespaceDeclarationSyntax>();
        List<MemberDeclarationSyntax> Other { get; } = new List<MemberDeclarationSyntax>();
        List<AttributeListSyntax> AttributeLists { get; } = new List<AttributeListSyntax>();
        List<ExternAliasDirectiveSyntax> Externs { get; } = new List<ExternAliasDirectiveSyntax>();

        public SourceAggregator AddSource(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            Usings.AddRange(compilationUnit.Usings);
            Namespaces.AddRange(compilationUnit.Members.OfType<NamespaceDeclarationSyntax>());
            Other.AddRange(compilationUnit.Members.Except(Namespaces));
            if (!_ignoreAssemblyAttributeLists)
                AttributeLists.AddRange(compilationUnit.AttributeLists.Where(al => al.Target.Identifier.Kind() == SyntaxKind.AssemblyKeyword));
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
                .AddUsings(Usings.Distinct(new UsingComparer()).OrderBy(u => u.Name.ToString()).ToArray())
                .AddAttributeLists(attributeList)
                .AddExterns(Externs.ToArray())
                .AddMembers(namespaces.ToArray())
                .AddMembers(Other.ToArray())
                .NormalizeWhitespace();

            return cs.ToString();
        }
    }
}
