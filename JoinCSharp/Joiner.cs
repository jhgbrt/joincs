using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp.RuntimeBinder;

namespace JoinCSharp
{
    public static class Joiner
    {
        public static string Join(
            IEnumerable<string> sources
            )
        {
            var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            // TODO how to sensibly determine which references must be added?
            var compilation = CSharpCompilation
                .Create("tmp")
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof (Enumerable).Assembly.Location), // System.Linq
                    MetadataReference.CreateFromFile(typeof (Binder).Assembly.Location) // Microsoft.CSharp
                )
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(syntaxTrees);

            // check the compiler result, just to ensure all references have been added
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                    return null;
                }

            }

            var models = (
                from syntaxTree in syntaxTrees
                let semanticModel = compilation.GetSemanticModel(syntaxTree)
                let compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot()
                select new
                {
                    model = semanticModel,
                    compilationUnit,
                    namespaceDeclarations = compilationUnit.Members.OfType<NamespaceDeclarationSyntax>(),
                    classesInGlobalNamespace = compilationUnit.Members.OfType<ClassDeclarationSyntax>()
                }
                ).ToList();

            var nameSpaces = (
                from x in models
                from nsdeclaration in x.namespaceDeclarations
                let ns = x.model.GetDeclaredSymbol(nsdeclaration)
                select new { ns, nsdeclaration }
                ).OrderBy(x => x.ns.Name)
                .GroupBy(x => x.ns);

            var usings = (
                from x in models
                from u in x.compilationUnit.Usings
                let symbolInfo = x.model.GetSymbolInfo(u.Name).Symbol as INamespaceSymbol
                select symbolInfo
                ).Distinct();

            var usingDirectives = usings
                .Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u.ToDisplayString())))
                .ToArray();

            var namespaceDeclarations = (
                from ns in nameSpaces
                select SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(ns.Key.ToDisplayString()))
                    .AddMembers(ns.SelectMany(x => x.nsdeclaration.Members).ToArray()) as MemberDeclarationSyntax
                ).ToArray();

            var classDeclarations = models
                .SelectMany(ns => ns.classesInGlobalNamespace)
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            var cs = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectives)
                .AddMembers(namespaceDeclarations)
                .AddMembers(classDeclarations)
                .NormalizeWhitespace();

            return cs.ToString();
        }
    }
}