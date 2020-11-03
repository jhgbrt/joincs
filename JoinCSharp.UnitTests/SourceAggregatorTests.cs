using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace JoinCSharp.UnitTests
{
    public class SourceAggregatorTests
    {
        private static string Process(string input, params string[] preprocessorSymbols)
        {
            return new SourceAggregator(true).AddSource(input.Preprocess(preprocessorSymbols)).GetResult();
        }

        [Fact]
        public void SimpleEnum()
        {
            var input = "enum MyEnum {A, B, C}";

            var result = Process(input);

            var expected = "enum MyEnum\r\n" +
                            "{\r\n" +
                            "    A,\r\n" +
                            "    B,\r\n" +
                            "    C\r\n" +
                            "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void EmptyClass_IsFormattedOnOneLine()
        {
            var input = "class SomeClass {}";

            var result = Process(input);

            var expected = "class SomeClass { }";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void NonEmptyClass_IsNotFormattedOnOneLine()
        {
            var input = "class SomeClass { private bool _b; }";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                "{\r\n" +
                "    private bool _b;\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void EmptyInterface_IsFormattedOnOneLine()
        {
            var input = "interface SomeClass {}";

            var result = Process(input);

            var expected = "interface SomeClass { }";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void NonEmptyInterface_IsNotFormattedOnOneLine()
        {
            var input = "interface SomeClass { private void M(); }";

            var result = Process(input);

            var expected = "interface SomeClass\r\n" +
                "{\r\n" +
                "    private void M();\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void EmptyMethod_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private void M(){} }";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                "{\r\n" +
                "    private void M() { }\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void ExpressionMethod_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private int M() => 1; }";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                "{\r\n" +
                "    private int M() => 1;\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void ExpressionProperty_IsFormattedOnOneLine()
        {

            var input = "class SomeClass { private int M => 1; }";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                "{\r\n" +
                "    private int M => 1;\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void AutoProperty_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private int M {get;set;} }";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                "{\r\n" +
                "    private int M { get; set; }\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void ReadonlyProperty_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private int M {get;} }";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                "{\r\n" +
                "    private int M { get; }\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ExternAlias()
        {
            var input = "extern alias SomeAlias;";

            var result = Process(input);

            var expected = "extern alias SomeAlias;";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void AssemblyAttribute()
        {
            var input = "using SomeNamespace;\r\n[assembly: SomeAttribute()]";

            var result = Process(input);

            var expected = "using SomeNamespace;\r\n\r\n[assembly: SomeAttribute()]";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void AssemblyAttributeList()
        {
            var input = "using SomeNamespace;\r\n[assembly: SomeAttribute(), MyAttribute()]";

            var result = Process(input);

            var expected = "using SomeNamespace;\r\n\r\n[assembly: MyAttribute(), SomeAttribute()]";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void MultipleAssemblyAttributes()
        {
            var input = new[]
            {
                "using SomeNamespace;\r\n" +
                "[assembly: SomeAttribute2()]\r\n",
                "[assembly: SomeAttribute1()]"
            };

            var result = input.Aggregate(true);

            var expected = "using SomeNamespace;\r\n" +
                           "\r\n" +
                           "[assembly: SomeAttribute1(), SomeAttribute2()]";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void IgnoreAssemblyAttributes()
        {
            var input = new[]
            {
                "using SomeNamespace;\r\n" +
                "[assembly: SomeAttribute2()]\r\n",
                "[assembly: SomeAttribute1()]"
            };

            var result = input.Aggregate(false);

            var expected = "using SomeNamespace;";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ClassInNamespaceWithUsing()
        {
            var input = "using Some.Using; namespace Some.Namespace { class SomeClass {} }";

            var result = Process(input);

            var expected = "using Some.Using;\r\n" +
                           "\r\n" +
                           "namespace Some.Namespace\r\n" +
                           "{\r\n" +
                           "    class SomeClass { }\r\n" +
                           "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void TwoSameUsingsAreGrouped()
        {
            var input = "using MyUsing;\r\nusing MyUsing;";
            var result = Process(input);
            var expected = "using MyUsing;";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void TwoDifferentUsingsAreOrdered()
        {
            var input = "using MyUsing2;\r\nusing MyUsing1;";
            var result = Process(input);
            const string expected = "using MyUsing1;\r\nusing MyUsing2;";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void StaticUsingInNamespace()
        {
            var input = "namespace Some.Namespace { using static SomeClass; }";

            var result = Process(input);

            var expected = "namespace Some.Namespace\r\n" +
                "{\r\n" +
                "    using static SomeClass;\r\n" +
                "}";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalIsStripped()
        {
            var input = "#if CONDITIONAL\r\nusing MyUsing;\r\n#endif";
            var result = Process(input, "CONDITIONAL");
            const string expected = "using MyUsing;";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalIsStrippedFromCode()
        {
            var input = "using MyUsing1;\r\n#if CONDITIONAL\r\nusing MyUsing;\r\n#endif";
            var result = Process(input, "CONDITIONAL");
            const string expected = "using MyUsing;\r\nusing MyUsing1;";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }


        [Fact]
        public void WhenCompilingWithPreprocessorDirective_ConditionalCodeIsRetained()
        {
            var input =
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n" +
                "}";

            var result = Process(input, new[] { "CONDITIONAL" });

            var expected = "namespace Abc.Def\r\n" +
                "{\r\n" +
                "    class ConditionalClass { }\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void SimpleUsing()
        {
            string input = "using Some.Using;";
            string expected = "using Some.Using;";

            var result = Process(input, new string[] { "CONDITIONAL" });

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalUsing_NoPreprocessorSymbols_UsingIsRemoved()
        {
            string input = "#if CONDITIONAL\r\n" +
                "using Some.Using;\r\n" +
                "#endif";

            var result = Process(input, Array.Empty<string>());
            var expected = string.Empty;
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalUsing_WithPreprocessorSymbol_UsingIsMaintained()
        {
            string input = "#if CONDITIONAL\r\n" +
                "using Some.Using;\r\n" +
                "#endif";

            string expected = "using Some.Using;";

            var result = Process(input, "CONDITIONAL");

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void WhenCompilingWithoutConditionalDirective_ConditionalCodeIsStrippedAway()
        {
            var input =
                "#if CONDITIONAL\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "#endif\r\n" +
                "using Some.Using1;\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n" +
                "}";

            var result = Process(input);

            var expected =
                "using Some.Using1;\r\n\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "}";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void ProcessUsings()
        {
            string input = "using MyUsing1;\r\nusing MyUsing2;";

            string result = Process(input);

            const string expected = "using MyUsing1;\r\nusing MyUsing2;";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalIsNotStrippedFromCode()
        {
            string input =
        "using MyUsing1;\r\n" +
        "#if CONDITIONAL\r\n" +
        "using MyUsing2;\r\n" +
        "#endif";
            string result = Process(input, "CONDITIONAL");
            const string expected = "using MyUsing1;\r\nusing MyUsing2;";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ClassWithComment()
        {
            var input = "// some comment\r\nclass SomeClass {}";

            var result = Process(input);

            var expected = "// some comment\r\n" +
                           "class SomeClass { }";

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalMethod_NoSymbols_MethodIsStripped()
        {
            string input = "class SomeClass {\r\n" +
                "#if CONDITIONAL\r\n" +
                "    void MyMethod()\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "#endif\r\n" +
                "}";

            string expected = "class SomeClass { }";

            var result = Process(input);

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalMethod_WithSymbols_MethodIsNotStripped()
        {
            string input = "class SomeClass {\r\n" +
                           "#if CONDITIONAL1\r\n" +
                           "    void MyMethod1()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "#endif\r\n" +
                           "#if CONDITIONAL2\r\n" +
                           "    void MyMethod2()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "#endif\r\n" +
                           "}";

            string expected = "class SomeClass\r\n" +
                              "{\r\n" +
                              "    void MyMethod2() { }\r\n" +
                              "}";

            var result = Process(input, "CONDITIONAL2");

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void ConditionalClass_WithDirective_ClassIsStripped()
        {
            string input = "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n" +
                "}";

            string expected = "namespace Abc.Def\r\n" +
                            "{\r\n" +
                            "    class ConditionalClass { }\r\n" +
                            "}";

            var result = Process(input, new string[] { "CONDITIONAL" });

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        public void ObjectInitializerShouldBeProperlyFormatted()
        {
            var input = "var p = new MyClass { SomeProperty = \"SomeValue\" }";

            var result = CSharpSyntaxTree.ParseText(input)
                .GetRoot()
                .NormalizeWhitespace()
                .ToFullString();

            var expected = "var p = new MyClass\r\n" +
                "{\r\n" +
                "    SomeProperty = \"SomeValue\"\r\n" +
                "}";

            Assert.Equal(expected, result);
        }

    }
}
