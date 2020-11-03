using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JoinCSharp
{

    static class MyFormatter
    {
        public static SyntaxNode Format(this SyntaxNode node)
            => node.NormalizeWhitespace().SingleLineProperties();

        public static SyntaxNode SingleLineProperties(this SyntaxNode node)
            => new SingleLineClassesAndPropertiesRewriter().Visit(node);

        class SingleLineClassesAndPropertiesRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
                => node.ExpressionBody is null && (node.AccessorList is null || node.AccessorList.Accessors.All(a => a.Body is null))
                ? node.FormatSingleLine()
                : base.VisitPropertyDeclaration(node);

            public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
            => node.ChildNodes().Any()
                ? base.VisitClassDeclaration(node)
                : node.FormatSingleLine();

            public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
            => node.HasBody()
                ? base.VisitMethodDeclaration(node)
                : node.FormatSingleLine();

            public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) 
            => node.ChildNodes().Any()
                ? base.VisitInterfaceDeclaration(node)
                : node.FormatSingleLine();

            [return: NotNullIfNotNull("node")]
            public override SyntaxNode? Visit(SyntaxNode? node)
            {
                return base.Visit(node);
            }
        }


        private static SyntaxNode FormatSingleLine(this SyntaxNode node)
            => node.NormalizeWhitespace(indentation: "", eol: " ")
                      .WithLeadingTrivia(node.GetLeadingTrivia())
                      .WithTrailingTrivia(node.GetTrailingTrivia());

        private static bool HasBody(this MethodDeclarationSyntax node)
            => (node.ExpressionBody is not null && node.ExpressionBody.ChildNodes().Any())
            || (node.Body is not null && node.Body.ChildNodes().Any());
    }
}
