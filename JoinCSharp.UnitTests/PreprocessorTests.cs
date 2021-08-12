using Xunit;
using Xunit.Abstractions;

namespace JoinCSharp.UnitTests;

static class Ex
{
    public static string Preprocess(this string s, ITestOutputHelper helper, params string[] directives)
    {
        helper.WriteLine("+=========+");
        helper.WriteLine("  INPUT:   ");
        helper.WriteLine("+=========+");
        helper.WriteLine(s);
        helper.WriteLine("");
        helper.WriteLine("directives: " + string.Join(",", directives));
        helper.WriteLine("");

        var result = string.Join(Environment.NewLine, s.ReadLines().Preprocess(null, directives));

        helper.WriteLine("+=========+");
        helper.WriteLine("  RESULT:  ");
        helper.WriteLine("+=========+");
        helper.WriteLine(result);
        return result;
    }
}

public class PreprocessorTests
{

    readonly ITestOutputHelper _helper;
    public PreprocessorTests(ITestOutputHelper helper)
    {
        _helper = helper;
    }

    [Fact]
    public void Preprocess_Empty_Remains()
    {
        string expected = string.Empty;
        string input = string.Empty;
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_OnlyConditional_BecomesEmpty()
    {
        string expected = string.Empty;
        string input =
            "#if FOO\r\n" +
            "FOO\r\n" +
            "#endif";
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_AdditionalWhitespace_BecomesEmpty()
    {
        string expected = string.Empty;
        string input =
            "   #if   FOO\t \r\n" +
            "FOO\r\n" +
            "\t #endif";
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_AdditionalWhitespace_Negative()
    {
        string expected = "FOO";
        string input =
            "   #if   !WHATEVER\t \r\n" +
            "FOO\r\n" +
            "\t #endif";
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_TrailingWhitespaceAfterElse()
    {
        string expected = "BAR";
        string input =
            "#if WHATEVER\r\n" +
            "FOO\r\n" +
            "#else\t\r\n" +
            "BAR\r\n" +
            "#endif ";
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_TrailingWhitespaceAfterEndIf()
    {
        string expected = "BAR";
        string input =
            "#if FOO\r\n" +
            "FOO\r\n" +
            "#else\r\n" +
            "BAR\r\n" +
            "#endif\t";
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_NoWhitespace_Negative_BecomesEmpty()
    {
        string expected = "FOO";
        string input =
            "#if !BAR\t \r\n" +
            "FOO\r\n" +
            "\t #endif";
        string result = input.Preprocess(_helper);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_InvalidDirective_NotTouched()
    {
        string input = "#if\r\n";
        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }
    [Fact]
    public void Preprocess_InvalidNegativeDirective_NotTouched()
    {
        string input = "#if !\r\n";
        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }
    [Fact]
    public void Preprocess_InvalidNegativeDirective2_NotTouched()
    {
        string input = "#if!\r\n";
        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }

    [Fact]
    public void Preprocess_NoConditionals_Remains()
    {
        string input =
            "class SomeClass {\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        var result = input.Preprocess(_helper);
        var expected = input;

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_WithConditionals_Stripped()
    {
        string input =
            "class SomeClass {\r\n" +
            "#if CONDITIONAL\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "#endif\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        string expected =
            "class SomeClass {\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        var result = input.Preprocess(_helper);

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_WithNegativeConditionals_Stripped()
    {
        string input =
            "class SomeClass {\r\n" +
            "#if !CONDITIONAL\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "#endif\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        string expected =
            "class SomeClass {\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        var result = input.Preprocess(_helper);

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Preprocess_WithNegativeConditionals_ConditionalSpecifed_NotStripped()
    {
        string input =
            "class SomeClass {\r\n" +
            "#if !CONDITIONAL\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "#endif\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        string expected =
            "class SomeClass {\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        var result = input.Preprocess(_helper, "CONDITIONAL");

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }


    [Fact]
    public void Preprocess_WithConditionals_DirectiveSpecified_Retained()
    {
        string input =
            "class SomeClass {\r\n" +
            "#if CONDITIONAL\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "#endif\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        string expected =
            "class SomeClass {\r\n" +
            "    void MyMethod1()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "    void MyMethod2()\r\n" +
            "    {\r\n" +
            "    }\r\n" +
            "}";

        var result = input.Preprocess(_helper, "CONDITIONAL");

        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Preprocess_IfElse()
    {
        var input =
            "#if FOO\r\nFOO\r\n" +
            "#elif BAR\r\nBAR\r\n" +
            "#else\r\nBAZ\r\n" +
            "#endif";

        var result = input.Preprocess(_helper, "FOO");
        var expected = "FOO";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Preprocess_TwoIfElseBlocks_BothAreProcessed()
    {
        var input =
            "#if FOO\r\nFOO\r\n#elif BAR\r\nBAR\r\n#else\r\nBAZ\r\n#endif\r\n\r\n" +
            "HELLO\r\n\r\n" +
            "#if FOO\r\nFOO\r\n#elif BAR\r\nBAR\r\n#else\r\nBAZ\r\n#endif";

        var result = input.Preprocess(_helper, "FOO");
        var expected = "FOO\r\n\r\nHELLO\r\n\r\nFOO";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Preprocess_IfInvalid()
    {
        var input = "#ifFOO";
        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }


    [Fact]
    public void Preprocess_IfElIf()
    {
        var input =
            "#if FOO\r\nFOO\r\n#elif BAR\r\nBAR\r\n#else\r\nBAZ\r\n#endif";

        var result = input.Preprocess(_helper, "BAR");
        var expected = "BAR";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }
    [Fact]
    public void Preprocess_IfElIfInvalid()
    {
        var input =
            "#if FOO\r\n" +
            "#elifBAR";

        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }
    [Fact]
    public void Preprocess_IfElse_2()
    {
        var input =
            "#if DEBUG\r\nDEBUG\r\n#else\r\nRELEASE\r\n#endif";

        var result = input.Preprocess(_helper, "DEBUG");
        var expected = "DEBUG";
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Preprocess_IfElseInvalid()
    {
        var input =
            "#if DEBUG\r\n#elseRELEASE\r\n#endif";

        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper, "DEBUG"));
    }

    [Theory]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#else\r\nBAS\r\n#endif\r\nBAT\r\n#endif\r\n",
        "FOO\r\nBAS\r\nBAT",
        "FOO"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#else\r\nBAS\r\n#endif\r\nBAT\r\n#endif\r\n",
        "FOO\r\nBAR\r\nBAT",
        "FOO", "BAR"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#else\r\nBAS\r\n#endif\r\nBAT\r\n#endif\r\n",
        "",
        "BAR"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#else\r\nBAS\r\n#endif\r\nBAT\r\n#endif\r\n",
        "",
        "BAQ"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#elif BAS\r\nBAS\r\n#else\r\nBAF\r\n#endif\r\nBAT\r\n#endif\r\n",
        "FOO\r\nBAF\r\nBAT",
        "FOO"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#if BAS\r\nBAS\r\n#endif\r\n#endif\r\nBAT\r\n#endif\r\n",
        "FOO\r\nBAR\r\nBAS\r\nBAT",
        "FOO", "BAR", "BAS"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#if BAS\r\nBAS\r\n#endif\r\n#endif\r\nBAT\r\n#endif\r\n",
        "FOO\r\nBAR\r\nBAT",
        "FOO", "BAR"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#elif BAR\r\nBAR\r\n#if BAS\r\nBAS\r\n#endif\r\nBAT\r\n#endif\r\n",
        "BAR\r\nBAS\r\nBAT",
        "BAR", "BAS"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#elif BAR\r\nBAR\r\n#if BAS\r\nBAS\r\n#endif\r\nBAT\r\n#endif\r\n",
        "BAR\r\nBAT",
        "BAR"
        )]
    [InlineData(
        "#if FOO\r\nFOO\r\n#if BAR\r\nBAR\r\n#else\r\nBAS\r\n#endif\r\n#elif BAQ\r\n#else\r\nBAT\r\n#endif\r\n",
        "FOO\r\nBAS",
        "FOO"
        )]
    public void Preprocess_NestedIfs(string input, string expected, params string[] directives)
    {
        var result = input.Preprocess(_helper, directives);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Theory]
    [InlineData("#if A")]
    [InlineData("#else")]
    [InlineData("#endif")]
    [InlineData("#elif B")]
    [InlineData("#warning w")]
    [InlineData("#line 0")]
    [InlineData("#region")]
    [InlineData("#endregion")]
    [InlineData("#define X")]
    [InlineData("#undef X")]
    public void ValidPreprocessorDirective(string input)
    {
        input.Preprocess(_helper);
    }
    [Theory]
    [InlineData("#error some message", "some message")]
    [InlineData("#error", "")]
    public void Error_Throws(string input, string expected)
    {
        var e = Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
        Assert.Equal(expected, e.Message);
    }

    [Theory]
    [InlineData("BLA\r\n#define FOO")]
    [InlineData("#if FOO\r\n#define FOO")]
    [InlineData("#if FOO\r\n#elif BAR\r\n#define FOO")]
    [InlineData("#if FOO\r\n#else\r\n#define FOO")]
    [InlineData("#if FOO\r\n#elif BAR\r\n#else BAT\r\n#endif\r\n#define FOO")]
    [InlineData("#if FOO\r\n#endif\r\n#define FOO")]
    public void Define_NotAtStart_Throws(string input)
    {
        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }

    [Fact]
    public void Define_AtStart_DoesNotThrow()
    {
        var input = "\r\n#define FOO\r\n#if FOO\r\nFOO\r\n#endif";
        var result = input.Preprocess(_helper);
        Assert.Equal("\r\nFOO", result, ignoreLineEndingDifferences: true);
    }

    [Theory]
    [InlineData("BLA\r\n#undef FOO")]
    [InlineData("#if FOO\r\n#undef FOO")]
    [InlineData("#if FOO\r\n#elif BAR\r\n#undef FOO")]
    [InlineData("#if FOO\r\n#else\r\n#undef FOO")]
    [InlineData("#if FOO\r\n#elif BAR\r\n#else BAT\r\n#endif\r\n#undef FOO")]
    [InlineData("#if FOO\r\n#endif\r\n#undef FOO")]
    public void UnDefine_NotAtStart_Throws(string input)
    {
        Assert.Throws<PreprocessorException>(() => input.Preprocess(_helper));
    }

    [Fact]
    public void UnDefine_AtStart_DoesNotThrow()
    {
        var input = "\r\n#undef FOO\r\n#if FOO\r\nFOO\r\n#endif";
        var result = input.Preprocess(_helper, "FOO");
        Assert.Equal("", result);
    }

}