using Xunit;
using Xunit.Abstractions;

using static System.Environment;

namespace JoinCSharp.UnitTests;

public class SourceAggregatorTests
{
    readonly ITestOutputHelper _output;
    public SourceAggregatorTests(ITestOutputHelper helper)
    {
        _output = helper;
    }

    private string Process(string input, params string[] preprocessorSymbols)
    {
        _output.WriteLine("INPUT: ");
        _output.WriteLine("=======");
        _output.WriteLine(input);
        var result = new SourceAggregator(true).AddSource(input.Preprocess(_output, preprocessorSymbols)).GetResult();
        _output.WriteLine("RESULT: ");
        _output.WriteLine("=======");
        _output.WriteLine(result);
        return result;
    }

    [Fact]
    public void SimpleEnum()
    {
        var input = "enum MyEnum {A, B, C}";

        var result = Process(input);

        var expected = "enum MyEnum" + NewLine + "" +
                        "{" + NewLine + "" +
                        "    A," + NewLine + "" +
                        "    B," + NewLine + "" +
                        "    C" + NewLine + "" +
                        "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void EmptyClass_IsFormattedOnOneLine()
    {
        var input = "class SomeClass {}";

        var result = Process(input);

        var expected = "class SomeClass { }";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void NonEmptyClass_IsNotFormattedOnOneLine()
    {
        var input = "class SomeClass { private bool _b; }";

        var result = Process(input);

        var expected = "class SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private bool _b;" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void EmptyInterface_IsFormattedOnOneLine()
    {
        var input = "interface SomeClass {}";

        var result = Process(input);

        var expected = "interface SomeClass { }";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void NonEmptyInterface_IsNotFormattedOnOneLine()
    {
        var input = "interface SomeClass { private void M(); }";

        var result = Process(input);

        var expected = "interface SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private void M();" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void EmptyMethod_IsFormattedOnOneLine()
    {
        var input = "class SomeClass { private void M(){} }";

        var result = Process(input);

        var expected = "class SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private void M() { }" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void ExpressionMethod_IsFormattedOnOneLine()
    {
        var input = "class SomeClass { private int M() => 1; }";

        var result = Process(input);

        var expected = "class SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private int M() => 1;" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void ExpressionProperty_IsFormattedOnOneLine()
    {

        var input = "class SomeClass { private int M => 1; }";

        var result = Process(input);

        var expected = "class SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private int M => 1;" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void AutoProperty_IsFormattedOnOneLine()
    {
        var input = "class SomeClass { private int M {get;set;} }";

        var result = Process(input);

        var expected = "class SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private int M { get; set; }" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void ReadonlyProperty_IsFormattedOnOneLine()
    {
        var input = "class SomeClass { private int M {get;} }";

        var result = Process(input);

        var expected = "class SomeClass" + NewLine + "" +
            "{" + NewLine + "" +
            "    private int M { get; }" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ExternAlias()
    {
        var input = "extern alias SomeAlias;";

        var result = Process(input);

        var expected = "extern alias SomeAlias;";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void AssemblyAttribute()
    {
        var input = "using SomeNamespace;" + NewLine + "[assembly: SomeAttribute()]";

        var result = Process(input);

        var expected = "using SomeNamespace;" + NewLine + "" + NewLine + "[assembly: SomeAttribute()]";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void AssemblyAttributeList()
    {
        var input = "using SomeNamespace;" + NewLine + "[assembly: SomeAttribute(), MyAttribute()]";

        var result = Process(input);

        var expected = "using SomeNamespace;" + NewLine + "" + NewLine + "[assembly: MyAttribute(), SomeAttribute()]";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void MultipleAssemblyAttributes()
    {
        var input = new[]
        {
            "using SomeNamespace;" + NewLine + "" +
            "[assembly: SomeAttribute2()]" + NewLine + "",
            "[assembly: SomeAttribute1()]"
        };

        var result = input.Aggregate(true);

        var expected = "using SomeNamespace;" + NewLine + "" +
                        "" + NewLine + "" +
                        "[assembly: SomeAttribute1(), SomeAttribute2()]";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void IgnoreAssemblyAttributes()
    {
        var input = new[]
        {
            "using SomeNamespace;" + NewLine + "" +
            "[assembly: SomeAttribute2()]" + NewLine + "",
            "[assembly: SomeAttribute1()]"
        };

        var result = input.Aggregate(false);

        var expected = "using SomeNamespace;";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ClassInNamespaceWithUsing()
    {
        var input = "using Some.Using; namespace Some.Namespace { class SomeClass {} }";

        var result = Process(input);

        var expected = "using Some.Using;" + NewLine + "" +
                        "" + NewLine + "" +
                        "namespace Some.Namespace" + NewLine + "" +
                        "{" + NewLine + "" +
                        "    class SomeClass { }" + NewLine + "" +
                        "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void TwoSameUsingsAreGrouped()
    {
        var input = "using MyUsing;\r\nusing MyUsing;";
        var result = Process(input);
        var expected = "using MyUsing;";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void TwoDifferentUsingsAreOrdered()
    {
        var input = "using MyUsing2;\r\nusing MyUsing1;";
        var result = Process(input);
        const string expected = "using MyUsing1;\r\nusing MyUsing2;";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void StaticUsingInNamespace()
    {
        var input = "namespace Some.Namespace { using static SomeClass; }";

        var result = Process(input);

        var expected = "namespace Some.Namespace" + NewLine + "" +
            "{" + NewLine + "" +
            "    using static SomeClass;" + NewLine + "" +
            "}";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalIsStripped()
    {
        var input = "#if CONDITIONAL\r\nusing MyUsing;" + NewLine + "#endif";
        var result = Process(input, "CONDITIONAL");
        const string expected = "using MyUsing;";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalIsStrippedFromCode()
    {
        var input = "using MyUsing1;" + NewLine + "#if CONDITIONAL\r\nusing MyUsing;" + NewLine + "#endif";
        var result = Process(input, "CONDITIONAL");
        const string expected = "using MyUsing;\r\nusing MyUsing1;";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }


    [Fact]
    public void WhenCompilingWithPreprocessorDirective_ConditionalCodeIsRetained()
    {
        var input =
            "namespace Abc.Def" + NewLine + "" +
            "{" + NewLine + "" +
            "#if CONDITIONAL" + NewLine + "" +
            "   class ConditionalClass{}" + NewLine + "" +
            "#endif" + NewLine + "" +
            "}";

        var result = Process(input, new[] { "CONDITIONAL" });

        var expected = "namespace Abc.Def" + NewLine + "" +
            "{" + NewLine + "" +
            "    class ConditionalClass { }" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void SimpleUsing()
    {
        string input = "using Some.Using;";
        string expected = "using Some.Using;";

        var result = Process(input, new string[] { "CONDITIONAL" });

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalUsing_NoPreprocessorSymbols_UsingIsRemoved()
    {
        string input = "#if CONDITIONAL" + NewLine + "" +
            "using Some.Using;" + NewLine + "" +
            "#endif";

        var result = Process(input, Array.Empty<string>());
        var expected = string.Empty;
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalUsing_WithPreprocessorSymbol_UsingIsMaintained()
    {
        string input = "#if CONDITIONAL" + NewLine + "" +
            "using Some.Using;" + NewLine + "" +
            "#endif";

        string expected = "using Some.Using;";

        var result = Process(input, "CONDITIONAL");

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WhenCompilingWithoutConditionalDirective_ConditionalCodeIsStrippedAway()
    {
        var input =
            "#if CONDITIONAL" + NewLine + "" +
            "using Some.ConditionalUsing;" + NewLine + "" +
            "#endif" + NewLine + "" +
            "using Some.Using1;" + NewLine + "" +
            "namespace Abc.Def" + NewLine + "" +
            "{" + NewLine + "" +
            "#if CONDITIONAL" + NewLine + "" +
            "   class ConditionalClass{}" + NewLine + "" +
            "#endif" + NewLine + "" +
            "}";

        var result = Process(input);

        var expected =
            "using Some.Using1;" + NewLine + "" + NewLine + "" +
            "namespace Abc.Def" + NewLine + "" +
            "{" + NewLine + "" +
            "}";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void ProcessUsings()
    {
        string input = "using MyUsing1;\r\nusing MyUsing2;";

        string result = Process(input);

        const string expected = "using MyUsing1;\r\nusing MyUsing2;";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void GlobalKeywordsAreRemovedFromUsings()
    {
        string input = "global using MyUsing;";

        string result = Process(input);

        const string expected = "using MyUsing;";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalIsNotStrippedFromCode()
    {
        string input =
    "using MyUsing1;" + NewLine + "" +
    "#if CONDITIONAL" + NewLine + "" +
    "using MyUsing2;" + NewLine + "" +
    "#endif";
        string result = Process(input, "CONDITIONAL");
        const string expected = "using MyUsing1;\r\nusing MyUsing2;";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ClassWithComment()
    {
        var input = "// some comment\r\nclass SomeClass {}";

        var result = Process(input);

        var expected = "// some comment" + NewLine + "" +
                        "class SomeClass { }";

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalMethod_NoSymbols_MethodIsStripped()
    {
        string input = "class SomeClass {" + NewLine + "" +
            "#if CONDITIONAL" + NewLine + "" +
            "    void MyMethod()" + NewLine + "" +
            "    {" + NewLine + "" +
            "    }" + NewLine + "" +
            "#endif" + NewLine + "" +
            "}";

        string expected = "class SomeClass { }";

        var result = Process(input);

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalMethod_WithSymbols_MethodIsNotStripped()
    {
        string input = "class SomeClass {" + NewLine + "" +
                        "#if CONDITIONAL1" + NewLine + "" +
                        "    void MyMethod1()" + NewLine + "" +
                        "    {" + NewLine + "" +
                        "    }" + NewLine + "" +
                        "#endif" + NewLine + "" +
                        "#if CONDITIONAL2" + NewLine + "" +
                        "    void MyMethod2()" + NewLine + "" +
                        "    {" + NewLine + "" +
                        "    }" + NewLine + "" +
                        "#endif" + NewLine + "" +
                        "}";

        string expected = "class SomeClass" + NewLine + "" +
                            "{" + NewLine + "" +
                            "    void MyMethod2() { }" + NewLine + "" +
                            "}";

        var result = Process(input, "CONDITIONAL2");

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void ConditionalClass_WithDirective_ClassIsStripped()
    {
        string input = "namespace Abc.Def" + NewLine + "" +
            "{" + NewLine + "" +
            "#if CONDITIONAL" + NewLine + "" +
            "   class ConditionalClass{}" + NewLine + "" +
            "#endif" + NewLine + "" +
            "}";

        string expected = "namespace Abc.Def" + NewLine + "" +
                        "{" + NewLine + "" +
                        "    class ConditionalClass { }" + NewLine + "" +
                        "}";

        var result = Process(input, new string[] { "CONDITIONAL" });

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
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

        var expected = "var p = new MyClass(a, b, c)" + NewLine +
            "{" + NewLine +
            "    Alfa = \"SomeValue\"," + NewLine +
            "    Bravo = OtherValue," + NewLine +
            "    Charlie = 5," + NewLine +
            "    Delta = \"SomeValue\"," + NewLine +
            "    Echo = OtherValue," + NewLine +
            "    Foxtrot = 5," + NewLine +
            "    Golf = \"SomeValue\"," + NewLine +
            "    Hotel = OtherValue," + NewLine +
            "    India = 5" + NewLine +
            "}";

        var result = Process(input);

        _output.WriteLine(result);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void EmptyObjectInitializerShouldBeProperlyFormatted()
    {
        var input = "var p = new MyClass()" + NewLine +
            "{" + NewLine +
            "}";

        var expected = "var p = new MyClass() {}";

        var result = Process(input);

        _output.WriteLine(result);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void SimpleObjectInitializerShouldBeProperlyFormatted()
    {
        var input = "var p = new MyClass(" + NewLine +
            ");";

        var expected = "var p = new MyClass();";

        var result = Process(input);

        _output.WriteLine(result);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void SwitchStatement()
    {
        string input = "class C" + NewLine +
            "{" + NewLine +
            "    public void M(char c) {" + NewLine +
            "    switch c" +
            "    { " +
            "        case 'x': return 1; " +
            "        case 'y': return 2;" + NewLine +
            "    }" + NewLine +
            "}" + NewLine +
            "}"
            ;

        string expected = "class C" + NewLine +
            "{" + NewLine +
            "    public void M(char c)" + NewLine +
            "    {" + NewLine +
            "        switch c" + NewLine +
            "        {" + NewLine +
            "            case 'x':" + NewLine +
            "                return 1;" + NewLine +
            "            case 'y':" + NewLine +
            "                return 2;" + NewLine +
            "        }" + NewLine +
            "    }" + NewLine +
            "}"
            ;

        var result = Process(input);

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void SwitchExpression()
    {
        string input = "class C" + NewLine +
            "{" + NewLine +
            "    public void M(char c) => c switch " + NewLine +
            "    { 'x' => 1, 'y' => 2" + NewLine +
            "    };" + NewLine +
            "}"
            ;

        string expected = "class C" + NewLine +
            "{" + NewLine +
            "    public void M(char c) => c switch" + NewLine +
            "    {" + NewLine +
            "        'x' => 1," + NewLine +
            "        'y' => 2" + NewLine +
            "    };" + NewLine +
            "}"
            ;

        var result = Process(input);

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
}
