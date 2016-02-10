using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JoinCSharp
{
    public static class Joiner
    {
        public static string Join(IEnumerable<string> sources)
        {
            var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            var models = (
                from syntaxTree in syntaxTrees
                let compilationUnit = (CompilationUnitSyntax) syntaxTree.GetRoot()
                select new
                {
                    compilationUnit,
                    namespaces = compilationUnit.Members.OfType<NamespaceDeclarationSyntax>(),
                    classes = compilationUnit.Members.OfType<ClassDeclarationSyntax>()
                }
                ).ToArray();

            var namespaces = (
                from x in models
                from @namespace in x.namespaces
                let name = @namespace.Name.ToString()
                orderby name
                group @namespace by name into ns
                select CreateOneNamespaceDeclaration(ns)
                ).ToArray();

            var usings = (
                from x in models
                from @using in x.compilationUnit.Usings
                let name = @using.Name.ToString()
                group @using by name into usingDeclarations
                select usingDeclarations.First()
                ).ToArray();

            var classes = (
                from item in models
                from c in item.classes
                select c as MemberDeclarationSyntax
                ).ToArray();

            var cs = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(namespaces)
                .AddMembers(classes)
                .NormalizeWhitespace();

            return cs.ToString();
        }

        private static MemberDeclarationSyntax CreateOneNamespaceDeclaration(IGrouping<string, NamespaceDeclarationSyntax> ns)
        {
            var nameSyntax = SyntaxFactory.ParseName(ns.Key);
            return SyntaxFactory
                .NamespaceDeclaration(nameSyntax)
                .AddMembers(ns.SelectMany(x => x.Members)
                .ToArray());
        }
    }
}