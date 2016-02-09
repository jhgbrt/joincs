using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JoinCSharp
{
    public static class Joiner
    {
        public static string Join(IEnumerable<SyntaxTree> syntaxTrees)
        {
            var models = (
                from syntaxTree in syntaxTrees
                let compilationUnit = (CompilationUnitSyntax) syntaxTree.GetRoot()
                select new
                {
                    compilationUnit,
                    namespaceDeclarations = compilationUnit.Members.OfType<NamespaceDeclarationSyntax>(),
                    classesInGlobalNamespace = compilationUnit.Members.OfType<ClassDeclarationSyntax>()
                }
                ).ToList();

            var namespaceDeclarations = (
                from x in models
                from nsdeclaration in x.namespaceDeclarations
                let name = nsdeclaration.Name.ToString()
                orderby name
                group nsdeclaration by name into nameSpaces
                select SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nameSpaces.Key))
                    .AddMembers(nameSpaces.SelectMany(x => x.Members).ToArray()) as MemberDeclarationSyntax
                ).ToArray();

            var usings = (
                from x in models
                from usingDeclaration in x.compilationUnit.Usings
                select usingDeclaration
                ).GroupBy(x => x.Name.ToString())
                .Select(x => x.First())
                .ToArray();


            var classDeclarations = models
                .SelectMany(ns => ns.classesInGlobalNamespace)
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            var cs = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(namespaceDeclarations)
                .AddMembers(classDeclarations)
                .NormalizeWhitespace();

            return cs.ToString();
        }
    }
}