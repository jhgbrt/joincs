using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JoinCSharp.UnitTests
{
    public class SourceAggregatorTests
    {
        ITestOutputHelper _output;
        public SourceAggregatorTests(ITestOutputHelper helper)
        {
            _output = helper;
        }

        private static string Process(string input, params string[] preprocessorSymbols)
        {
            return new SourceAggregator(true).AddSource(input.Preprocess(preprocessorSymbols)).GetResult();
        }

        [Fact]
        public void SimpleEnum()
        {
            var input = "enum MyEnum {A, B, C}";

            var result = Process(input);

            var expected = "enum MyEnum" + Environment.NewLine + "" +
                            "{" + Environment.NewLine + "" +
                            "    A," + Environment.NewLine + "" +
                            "    B," + Environment.NewLine + "" +
                            "    C" + Environment.NewLine + "" +
                            "}";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void EmptyClass_IsFormattedOnOneLine()
        {
            var input = "class SomeClass {}";

            var result = Process(input);

            var expected = "class SomeClass { }";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void NonEmptyClass_IsNotFormattedOnOneLine()
        {
            var input = "class SomeClass { private bool _b; }";

            var result = Process(input);

            var expected = "class SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private bool _b;" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void EmptyInterface_IsFormattedOnOneLine()
        {
            var input = "interface SomeClass {}";

            var result = Process(input);

            var expected = "interface SomeClass { }";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void NonEmptyInterface_IsNotFormattedOnOneLine()
        {
            var input = "interface SomeClass { private void M(); }";

            var result = Process(input);

            var expected = "interface SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private void M();" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void EmptyMethod_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private void M(){} }";

            var result = Process(input);

            var expected = "class SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private void M() { }" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void ExpressionMethod_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private int M() => 1; }";

            var result = Process(input);

            var expected = "class SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private int M() => 1;" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void ExpressionProperty_IsFormattedOnOneLine()
        {

            var input = "class SomeClass { private int M => 1; }";

            var result = Process(input);

            var expected = "class SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private int M => 1;" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void AutoProperty_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private int M {get;set;} }";

            var result = Process(input);

            var expected = "class SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private int M { get; set; }" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void ReadonlyProperty_IsFormattedOnOneLine()
        {
            var input = "class SomeClass { private int M {get;} }";

            var result = Process(input);

            var expected = "class SomeClass" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    private int M { get; }" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExternAlias()
        {
            var input = "extern alias SomeAlias;";

            var result = Process(input);

            var expected = "extern alias SomeAlias;";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void AssemblyAttribute()
        {
            var input = "using SomeNamespace;" + Environment.NewLine + "[assembly: SomeAttribute()]";

            var result = Process(input);

            var expected = "using SomeNamespace;" + Environment.NewLine + "" + Environment.NewLine + "[assembly: SomeAttribute()]";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void AssemblyAttributeList()
        {
            var input = "using SomeNamespace;" + Environment.NewLine + "[assembly: SomeAttribute(), MyAttribute()]";

            var result = Process(input);

            var expected = "using SomeNamespace;" + Environment.NewLine + "" + Environment.NewLine + "[assembly: MyAttribute(), SomeAttribute()]";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void MultipleAssemblyAttributes()
        {
            var input = new[]
            {
                "using SomeNamespace;" + Environment.NewLine + "" +
                "[assembly: SomeAttribute2()]" + Environment.NewLine + "",
                "[assembly: SomeAttribute1()]"
            };

            var result = input.Aggregate(true);

            var expected = "using SomeNamespace;" + Environment.NewLine + "" +
                           "" + Environment.NewLine + "" +
                           "[assembly: SomeAttribute1(), SomeAttribute2()]";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void IgnoreAssemblyAttributes()
        {
            var input = new[]
            {
                "using SomeNamespace;" + Environment.NewLine + "" +
                "[assembly: SomeAttribute2()]" + Environment.NewLine + "",
                "[assembly: SomeAttribute1()]"
            };

            var result = input.Aggregate(false);

            var expected = "using SomeNamespace;";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ClassInNamespaceWithUsing()
        {
            var input = "using Some.Using; namespace Some.Namespace { class SomeClass {} }";

            var result = Process(input);

            var expected = "using Some.Using;" + Environment.NewLine + "" +
                           "" + Environment.NewLine + "" +
                           "namespace Some.Namespace" + Environment.NewLine + "" +
                           "{" + Environment.NewLine + "" +
                           "    class SomeClass { }" + Environment.NewLine + "" +
                           "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void TwoSameUsingsAreGrouped()
        {
            var input = "using MyUsing;\r\nusing MyUsing;";
            var result = Process(input);
            var expected = "using MyUsing;";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void TwoDifferentUsingsAreOrdered()
        {
            var input = "using MyUsing2;\r\nusing MyUsing1;";
            var result = Process(input);
            const string expected = "using MyUsing1;\r\nusing MyUsing2;";
            Assert.Equal(expected, result);
        }
        [Fact]
        public void StaticUsingInNamespace()
        {
            var input = "namespace Some.Namespace { using static SomeClass; }";

            var result = Process(input);

            var expected = "namespace Some.Namespace" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    using static SomeClass;" + Environment.NewLine + "" +
                "}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalIsStripped()
        {
            var input = "#if CONDITIONAL\r\nusing MyUsing;" + Environment.NewLine + "#endif";
            var result = Process(input, "CONDITIONAL");
            const string expected = "using MyUsing;";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalIsStrippedFromCode()
        {
            var input = "using MyUsing1;" + Environment.NewLine + "#if CONDITIONAL\r\nusing MyUsing;" + Environment.NewLine + "#endif";
            var result = Process(input, "CONDITIONAL");
            const string expected = "using MyUsing;\r\nusing MyUsing1;";

            Assert.Equal(expected, result);
        }


        [Fact]
        public void WhenCompilingWithPreprocessorDirective_ConditionalCodeIsRetained()
        {
            var input =
                "namespace Abc.Def" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "   class ConditionalClass{}" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "}";

            var result = Process(input, new[] { "CONDITIONAL" });

            var expected = "namespace Abc.Def" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "    class ConditionalClass { }" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SimpleUsing()
        {
            string input = "using Some.Using;";
            string expected = "using Some.Using;";

            var result = Process(input, new string[] { "CONDITIONAL" });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalUsing_NoPreprocessorSymbols_UsingIsRemoved()
        {
            string input = "#if CONDITIONAL" + Environment.NewLine + "" +
                "using Some.Using;" + Environment.NewLine + "" +
                "#endif";

            var result = Process(input, Array.Empty<string>());
            var expected = string.Empty;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalUsing_WithPreprocessorSymbol_UsingIsMaintained()
        {
            string input = "#if CONDITIONAL" + Environment.NewLine + "" +
                "using Some.Using;" + Environment.NewLine + "" +
                "#endif";

            string expected = "using Some.Using;";

            var result = Process(input, "CONDITIONAL");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenCompilingWithoutConditionalDirective_ConditionalCodeIsStrippedAway()
        {
            var input =
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "using Some.ConditionalUsing;" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "using Some.Using1;" + Environment.NewLine + "" +
                "namespace Abc.Def" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "   class ConditionalClass{}" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "}";

            var result = Process(input);

            var expected =
                "using Some.Using1;" + Environment.NewLine + "" + Environment.NewLine + "" +
                "namespace Abc.Def" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "}";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void ProcessUsings()
        {
            string input = "using MyUsing1;\r\nusing MyUsing2;";

            string result = Process(input);

            const string expected = "using MyUsing1;\r\nusing MyUsing2;";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalIsNotStrippedFromCode()
        {
            string input =
        "using MyUsing1;" + Environment.NewLine + "" +
        "#if CONDITIONAL" + Environment.NewLine + "" +
        "using MyUsing2;" + Environment.NewLine + "" +
        "#endif";
            string result = Process(input, "CONDITIONAL");
            const string expected = "using MyUsing1;\r\nusing MyUsing2;";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ClassWithComment()
        {
            var input = "// some comment\r\nclass SomeClass {}";

            var result = Process(input);

            var expected = "// some comment" + Environment.NewLine + "" +
                           "class SomeClass { }";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalMethod_NoSymbols_MethodIsStripped()
        {
            string input = "class SomeClass {" + Environment.NewLine + "" +
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "    void MyMethod()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "}";

            string expected = "class SomeClass { }";

            var result = Process(input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalMethod_WithSymbols_MethodIsNotStripped()
        {
            string input = "class SomeClass {" + Environment.NewLine + "" +
                           "#if CONDITIONAL1" + Environment.NewLine + "" +
                           "    void MyMethod1()" + Environment.NewLine + "" +
                           "    {" + Environment.NewLine + "" +
                           "    }" + Environment.NewLine + "" +
                           "#endif" + Environment.NewLine + "" +
                           "#if CONDITIONAL2" + Environment.NewLine + "" +
                           "    void MyMethod2()" + Environment.NewLine + "" +
                           "    {" + Environment.NewLine + "" +
                           "    }" + Environment.NewLine + "" +
                           "#endif" + Environment.NewLine + "" +
                           "}";

            string expected = "class SomeClass" + Environment.NewLine + "" +
                              "{" + Environment.NewLine + "" +
                              "    void MyMethod2() { }" + Environment.NewLine + "" +
                              "}";

            var result = Process(input, "CONDITIONAL2");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConditionalClass_WithDirective_ClassIsStripped()
        {
            string input = "namespace Abc.Def" + Environment.NewLine + "" +
                "{" + Environment.NewLine + "" +
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "   class ConditionalClass{}" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "}";

            string expected = "namespace Abc.Def" + Environment.NewLine + "" +
                            "{" + Environment.NewLine + "" +
                            "    class ConditionalClass { }" + Environment.NewLine + "" +
                            "}";

            var result = Process(input, new string[] { "CONDITIONAL" });

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ObjectInitializerShouldBeProperlyFormatted()
        {
            var input = "var p = new MyClass(a,b,c)" +
                "{" +
                "  Alfa = \"SomeValue\", " +
                "  Bravo = OtherValue," +
                "  Charlie = 5," +
                "  Delta = \"SomeValue\", " +
                "  Echo = OtherValue," +
                "  Foxtrot = 5," +
                "  Golf = \"SomeValue\", " +
                "  Hotel = OtherValue," +
                "  India = 5" +
                "}";

            var expected = "var p = new MyClass(a, b, c)" + Environment.NewLine +
                "{" +                              Environment.NewLine +
                "    Alfa = \"SomeValue\"," +    Environment.NewLine +
                "    Bravo = OtherValue," +       Environment.NewLine +
                "    Charlie = 5," +              Environment.NewLine +
                "    Delta = \"SomeValue\"," +   Environment.NewLine +
                "    Echo = OtherValue," +        Environment.NewLine +
                "    Foxtrot = 5," +              Environment.NewLine +
                "    Golf = \"SomeValue\"," +    Environment.NewLine +
                "    Hotel = OtherValue," +       Environment.NewLine +
                "    India = 5" +                Environment.NewLine +
                "}";

            var result = Process(input);

            _output.WriteLine(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EmptyObjectInitializerShouldBeProperlyFormatted()
        {
            var input = "var p = new MyClass() {}";

            var expected = "var p = new MyClass() {}";

            var result = Process(input);

            _output.WriteLine(result);
            Assert.Equal(expected, result);
        }
        [Fact]
        public void SimpleObjectInitializerShouldBeProperlyFormatted()
        {
            var input = "var p = new MyClass();";

            var expected = "var p = new MyClass();";

            var result = Process(input);

            _output.WriteLine(result);
            Assert.Equal(expected, result);
        }
    }
}
