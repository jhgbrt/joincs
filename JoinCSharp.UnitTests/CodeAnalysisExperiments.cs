using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace JoinCSharp.UnitTests;

public class CodeAnalysisExperiments
{

    [Fact]
    public void Visitor()
    {
        string input = "class SomeClass {" + Environment.NewLine + "" +
            "#if CONDITIONAL" + Environment.NewLine + "" +
            "    void MyMethod1()" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "#endif" + Environment.NewLine + "" +
            "    void MyMethod2()" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "}";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(input);
        //CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
        //SyntaxList<MemberDeclarationSyntax> members = compilationUnit.Members;

        MyVisitor walker = new();
        walker.Visit(syntaxTree.GetRoot());

        var rewriter = new CSharpRemoveRegionsAndDirectivesRewriter();
        var result = rewriter.Visit(syntaxTree.GetRoot());

        Console.WriteLine(result);
    }

    class CSharpRemoveRegionsAndDirectivesRewriter : CSharpSyntaxRewriter
    {
        public CSharpRemoveRegionsAndDirectivesRewriter()
            : base(true)
        {
        }

        public override SyntaxNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node) => SyntaxFactory.SkippedTokensTrivia();

        public override SyntaxNode VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node) => SyntaxFactory.SkippedTokensTrivia();

        public override SyntaxNode VisitDefineDirectiveTrivia(DefineDirectiveTriviaSyntax node) => SyntaxFactory.SkippedTokensTrivia();

        public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node) => SyntaxFactory.SkippedTokensTrivia();

        public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node) => SyntaxFactory.SkippedTokensTrivia();
    }


    private class MyVisitor : CSharpSyntaxWalker
    {
        private int Tabs;
        public override void Visit(SyntaxNode node)
        {
            Tabs++;
            string indents = new ('\t', Tabs);
            Console.WriteLine($"{indents} {node.Kind()} ({node})");
            base.Visit(node);
            Tabs--;
        }
    }
}
