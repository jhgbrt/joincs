using System;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JoinCSharp.UnitTests
{

    static class Ex
    {
        public static string Preprocess(this string s, ITestOutputHelper helper,params string[] directives)
        {
            helper.WriteLine("+=========+");
            helper.WriteLine("| INPUT:  |");
            helper.WriteLine("+=========+");
            helper.WriteLine(s);
            helper.WriteLine("");
            helper.WriteLine("directives: " + string.Join(",", directives));
            helper.WriteLine("");

            var result = string.Join(Environment.NewLine, s.ReadLines().Preprocess(helper.WriteLine, directives));

            helper.WriteLine("+=========+");
            helper.WriteLine("| RESULT: |");
            helper.WriteLine("+=========+");
            helper.WriteLine(result);
            return result;
        }
    }
    public class ExtensionTests
    {

        ITestOutputHelper _helper;
        public ExtensionTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void IsBelow_FileInFolder_True()
        {
            var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
            var root = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").DirectoryInfo;
            Assert.True(fileInfo.SitsBelow(root));
        }
        [Fact]
        public void IsBelow_FileInFolderBelow_True()
        {
            var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
            var root = PathBuilder.FromRoot().WithSubFolders("Users").DirectoryInfo;
            Assert.True(fileInfo.SitsBelow(root));
        }
        [Fact]
        public void IsBelow_FileInRootFolderBelow_True()
        {
            var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
            var root = PathBuilder.FromRoot().DirectoryInfo;
            Assert.True(fileInfo.SitsBelow(root));
        }
        [Fact]
        public void IsBelow_FileOtherFolderBelow_False()
        {
            var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
            var root = PathBuilder.FromRoot().WithSubFolders("Users", "Jane").DirectoryInfo;
            Assert.False(fileInfo.SitsBelow(root));
        }

        [Fact]
        public void Except_FiltersFileInSubfolders()
        {
            var input = new[]
            {
                PathBuilder.FromRoot().WithSubFolders("A", "AA").WithFileName("AAA.txt").FileInfo,
                PathBuilder.FromRoot().WithSubFolders("A", "AB").WithFileName("AAB.txt").FileInfo,
                PathBuilder.FromRoot().WithSubFolders("A", "AC").WithFileName("AAC.txt").FileInfo,
                PathBuilder.FromRoot().WithSubFolders("A", "AD").WithFileName("AAD.txt").FileInfo,
                PathBuilder.FromRoot().WithSubFolders("A", "AD").WithFileName("AAE.txt").FileInfo,
                PathBuilder.FromRoot().WithSubFolders("A", "AE").WithFileName("AAF.txt").FileInfo
            };

            var rootDir = PathBuilder.FromRoot().WithSubFolders("A").DirectoryInfo;
            var subdirs = new[] { "AB", "AD" }.Select(rootDir.SubFolder).ToArray();

            var result = input.Except(subdirs).Select(f => f.FullName).OrderBy(a => a).ToArray();

            var expected = new[]
            {
                PathBuilder.FromRoot().WithSubFolders("A", "AA").WithFileName("AAA.txt").FileInfo.FullName,
                PathBuilder.FromRoot().WithSubFolders("A", "AC").WithFileName("AAC.txt").FileInfo.FullName,
                PathBuilder.FromRoot().WithSubFolders("A", "AE").WithFileName("AAF.txt").FileInfo.FullName
            };

            Assert.Equal(expected, result);
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
                "#if FOO" + Environment.NewLine +
                "FOO" + Environment.NewLine +
                "#endif";
            string result = input.Preprocess(_helper);
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_BecomesEmpty()
        {
            string expected = string.Empty;
            string input =
                "   #if   FOO\t " + Environment.NewLine +
                "FOO" + Environment.NewLine +
                "\t #endif";
            string result = input.Preprocess(_helper);
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_Negative()
        {
            string expected = "FOO";
            string input =
                "   #if   !WHATEVER\t " + Environment.NewLine +
                "FOO" + Environment.NewLine +
                "\t #endif";
            string result = input.Preprocess(_helper);
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_TrailingWhitespaceAfterElse()
        {
            string expected = "BAR";
            string input =
                "#if WHATEVER" + Environment.NewLine +
                "FOO" + Environment.NewLine +
                "#else\t" + Environment.NewLine +
                "BAR" + Environment.NewLine +
                "#endif ";
            string result = input.Preprocess(_helper);
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_TrailingWhitespaceAfterEndIf()
        {
            string expected = "BAR";
            string input =
                "#if FOO" + Environment.NewLine +
                "FOO" + Environment.NewLine +
                "#else" + Environment.NewLine +
                "BAR" + Environment.NewLine +
                "#endif\t";
            string result = input.Preprocess(_helper);
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_NoWhitespace_Negative_BecomesEmpty()
        {
            string expected = "FOO";
            string input =
                "#if !BAR\t " + Environment.NewLine +
                "FOO" + Environment.NewLine +
                "\t #endif";
            string result = input.Preprocess(_helper);
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_InvalidDirective_NotTouched()
        {
            string input = "#if" + Environment.NewLine;
            Assert.Throws<InvalidPreprocessorDirectiveException>(() => input.Preprocess(_helper));
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective_NotTouched()
        {
            string input = "#if !" + Environment.NewLine;
            Assert.Throws<InvalidPreprocessorDirectiveException>(() => input.Preprocess(_helper));
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective2_NotTouched()
        {
            string input = "#if!" + Environment.NewLine;
            Assert.Throws<InvalidPreprocessorDirectiveException>(() => input.Preprocess(_helper));
        }

        [Fact]
        public void Preprocess_NoConditionals_Remains()
        {
            string input =
                "class SomeClass {" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            var result = input.Preprocess(_helper);
            var expected = input;

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_WithConditionals_Stripped()
        {
            string input = 
                "class SomeClass {" + Environment.NewLine + "" +
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            string expected = 
                "class SomeClass {" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            var result = input.Preprocess(_helper);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_WithNegativeConditionals_Stripped()
        {
            string input = 
                "class SomeClass {" + Environment.NewLine + "" +
                "#if !CONDITIONAL" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            string expected = 
                "class SomeClass {" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            var result = input.Preprocess(_helper);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Preprocess_WithNegativeConditionals_ConditionalSpecifed_NotStripped()
        {
            string input = 
                "class SomeClass {" + Environment.NewLine + "" +
                "#if !CONDITIONAL" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            string expected = 
                "class SomeClass {" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            var result = input.Preprocess(_helper, "CONDITIONAL");

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }


        [Fact]
        public void Preprocess_WithConditionals_DirectiveSpecified_Retained()
        {
            string input = 
                "class SomeClass {" + Environment.NewLine + "" +
                "#if CONDITIONAL" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            string expected = 
                "class SomeClass {" + Environment.NewLine + "" +
                "    void MyMethod1()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "    void MyMethod2()" + Environment.NewLine + "" +
                "    {" + Environment.NewLine + "" +
                "    }" + Environment.NewLine + "" +
                "}";

            var result = input.Preprocess(_helper, "CONDITIONAL");

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Preprocess_IfElse()
        {
            var input = 
                "#if FOO" + Environment.NewLine + "" +
                "FOO" + Environment.NewLine + "" +
                "#elif BAR" + Environment.NewLine + "" +
                "BAR" + Environment.NewLine + "" +
                "#else" + Environment.NewLine + "" +
                "BAZ" + Environment.NewLine + "" +
                "#endif";

            var result = input.Preprocess(_helper, "FOO");
            var expected = "FOO";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Preprocess_TwoIfElseBlocks_BothAreProcessed()
        {
            var input =
                "#if FOO" + Environment.NewLine + "" +
                "FOO" + Environment.NewLine + "" +
                "#elif BAR" + Environment.NewLine + "" +
                "BAR" + Environment.NewLine + "" +
                "#else" + Environment.NewLine + "" +
                "BAZ" + Environment.NewLine + "" +
                "#endif" + Environment.NewLine + "" +
                "" + Environment.NewLine + "" +
                "HELLO" + Environment.NewLine + "" +
                "" + Environment.NewLine + "" +
                "#if FOO" + Environment.NewLine + "" +
                "FOO" + Environment.NewLine + "" +
                "#elif BAR" + Environment.NewLine + "" +
                "BAR" + Environment.NewLine + "" +
                "#else" + Environment.NewLine + "" +
                "BAZ" + Environment.NewLine + "" +
                "#endif";
            ;

            var result = input.Preprocess(_helper, "FOO");
            var expected = "FOO\r\n\r\nHELLO\r\n\r\nFOO";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Preprocess_IfInvalid()
        {
            var input = "#ifFOO";
            Assert.Throws<InvalidPreprocessorDirectiveException>(() => input.Preprocess(_helper));
        }


        [Fact]
        public void Preprocess_IfElIf()
        {
            var input =
               "#if FOO" + Environment.NewLine + "" +
               "FOO" + Environment.NewLine + "" +
               "#elif BAR" + Environment.NewLine + "" +
               "BAR" + Environment.NewLine + "" +
               "#else" + Environment.NewLine + "" +
               "BAZ" + Environment.NewLine + "" +
               "#endif";

            var result = input.Preprocess(_helper, "BAR");
            var expected = "BAR";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_IfElIfInvalid()
        {
            var input =
               "#if FOO" + Environment.NewLine + "" +
               "#elifBAR";

            Assert.Throws<InvalidPreprocessorDirectiveException>(() => input.Preprocess(_helper));
        }
        [Fact]
        public void Preprocess_IfElse_2()
        {
            var input = 
                "#if DEBUG" + Environment.NewLine + "" +
                "DEBUG" + Environment.NewLine + "" +
                "#else" + Environment.NewLine + "" +
                "RELEASE" + Environment.NewLine + "" +
                "#endif";

            var result = input.Preprocess(_helper, "DEBUG");
            var expected = "DEBUG";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
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
    }
}