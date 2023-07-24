using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System.Diagnostics.CodeAnalysis;

namespace JoinCSharp;

static class MyFormatter
{
    static readonly AdhocWorkspace workspace = new();
    static readonly OptionSet options = workspace.Options
        .WithChangedOption(CSharpFormattingOptions.WrappingPreserveSingleLine, false);

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

        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) => node switch
        {
            { Initializer: null } => base.VisitObjectCreationExpression(node),
            { Initializer.Expressions.Count: 0 } => node.FormatSingleLine(),
            _ => Formatter.Format(node, workspace, options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, true))
        };

        [return: NotNullIfNotNull(nameof(node))]
        public override SyntaxNode? Visit(SyntaxNode? node) => base.Visit(node);
    }

    private static SyntaxNode FormatSingleLine(this SyntaxNode node)
        => node.NormalizeWhitespace(indentation: "", eol: " ")
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithTrailingTrivia(node.GetTrailingTrivia());

    private static bool HasBody(this MethodDeclarationSyntax node)
        => (node.ExpressionBody is not null && node.ExpressionBody.ChildNodes().Any())
        || (node.Body is not null && node.Body.ChildNodes().Any());
}
