using System;
using System.Linq;
using System.Text;
using Xunit;

namespace JoinCSharp.UnitTests
{

    static class Ex
    {
        public static string Preprocess(this string s, params string[] directives)
        {
            return string.Join(Environment.NewLine, s.ReadLines().Preprocess(directives));
        }
    }
    public class ExtensionTests
    {
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
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_OnlyConditional_BecomesEmpty()
        {
            string expected = string.Empty;
            string input =
                "#if WHATEVER" + Environment.NewLine +
                "#endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_BecomesEmpty()
        {
            string expected = string.Empty;
            string input =
                "   #if   WHATEVER\t " + Environment.NewLine +
                "\t #endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_Negative_BecomesEmpty()
        {
            string expected = string.Empty;
            string input =
                "   #if   !WHATEVER\t " + Environment.NewLine +
                "\t #endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_NoWhitespace_Negative_BecomesEmpty()
        {
            string expected = string.Empty;
            string input =
                "#if !WHATEVER\t " + Environment.NewLine +
                "\t #endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_InvalidDirective_NotTouched()
        {
            string expected =
                "#if" + Environment.NewLine +
                "#endif";
            string input =
                "#if" + Environment.NewLine +
                "#endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective_NotTouched()
        {
            string expected =
                "#if !" + Environment.NewLine +
                "#endif";
            string input =
                "#if !" + Environment.NewLine +
                "#endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective2_NotTouched()
        {
            string expected =
                "#if!" + Environment.NewLine +
                "#endif";
            string input =
                "#if!" + Environment.NewLine +
                "#endif";
            string result = input.Preprocess();
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
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

            var result = input.Preprocess();
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

            var result = input.Preprocess();

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

            var result = input.Preprocess();

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

            var result = input.Preprocess("CONDITIONAL");

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

            var result = input.Preprocess("CONDITIONAL");

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

            var result = input.Preprocess("FOO");
            var expected = "FOO";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Preprocess_IfElse2()
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

            var result = input.Preprocess("FOO");
            var expected = "FOO\r\n\r\nHELLO\r\n\r\nFOO";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Preprocess_IfInvalid()
        {
            var input =
                "#ifFOO" + Environment.NewLine + "" +
                "FOO" + Environment.NewLine + "" +
                "#elif BAR" + Environment.NewLine + "" +
                "BAR" + Environment.NewLine + "" +
                "#else" + Environment.NewLine + "" +
                "BAZ" + Environment.NewLine + "" +
                "#endif";

            var result = input.Preprocess("FOO");
            var expected = input;
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
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

            var result = input.Preprocess("BAR");
            var expected = "BAR";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        [Fact]
        public void Preprocess_IfElIfInvalid()
        {
            var input =
               "#if FOO" + Environment.NewLine + "" +
               "FOO" + Environment.NewLine + "" +
               "#elifBAR" + Environment.NewLine + "" +
               "BAR" + Environment.NewLine + "" +
               "#else" + Environment.NewLine + "" +
               "BAZ" + Environment.NewLine + "" +
               "#endif";

            var result = input.Preprocess("BAR");
            var expected = "BAZ";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
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

            var result = input.Preprocess("DEBUG");
            var expected = "DEBUG";
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
    }
}