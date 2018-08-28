using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JoinCSharp.UnitTests
{
    [TestClass]
    public class CodeAnalysisExperiments
    {
        [TestMethod]
        public void Visitor()
        {
            string input = "class SomeClass {\r\n" +
                "#if CONDITIONAL\r\n" +
                "    void MyMethod1()\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "#endif\r\n" +
                "    void MyMethod2()\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(input);
            CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            SyntaxList<MemberDeclarationSyntax> members = compilationUnit.Members;

            MyVisitor walker = new MyVisitor();
            walker.Visit(syntaxTree.GetRoot());
        }

        private class MyVisitor : CSharpSyntaxWalker
        {
            private int Tabs = 0;
            public override void Visit(SyntaxNode node)
            {
                Tabs++;
                string indents = new string('\t', Tabs);
                Console.WriteLine($"{indents} {node.Kind()} ({node.ToString()})");
                base.Visit(node);
                Tabs--;
            }
        }
    }
}
