using System;
using System.Linq;
using System.Text;
using Xunit;

namespace JoinCSharp.UnitTests
{

    static class Ex
    {
        public static string HandleCrLf(this string s)
            => s.Replace("\r\n", Environment.NewLine);

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
            var subdirs = new[] { "AB", "AD"}.Select(rootDir.SubFolder).ToArray();

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
            string result = string.Empty.Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_OnlyConditional_BecomesEmpty()
        {
            string expected = string.Empty;
            string result = "#if WHATEVER\r\n#endif".Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_BecomesEmpty()
        {
            string expected = string.Empty;
            string result = "   #if   WHATEVER\t \r\n\t #endif".Preprocess();

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_Negative_BecomesEmpty()
        {
            string expected = string.Empty;
            string result = "   #if   !WHATEVER\t \r\n\t #endif".Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_NoWhitespace_Negative_BecomesEmpty()
        {
            string expected = string.Empty;
            string result = "#if !WHATEVER\t \r\n\t #endif".Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_InvalidDirective_NotTouched()
        {
            const string expected = "#if\r\n#endif";
            string result = "#if\r\n#endif".Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective_NotTouched()
        {
            const string expected = "#if !\r\n#endif";
            string result = "#if !\r\n#endif".Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective2_NotTouched()
        {
            const string expected = "#if!\r\n#endif";
            string result = "#if!\r\n#endif".Preprocess();
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void Preprocess_NoConditionals_Remains()
        {
            string input = "class SomeClass {\r\n" +
                           "    void MyMethod1()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "    void MyMethod2()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "}";

            var result = input.Preprocess();
            var expected = input;
            
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_WithConditionals_Stripped()
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

            string expected = "class SomeClass {\r\n" +
                              "    void MyMethod2()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "}";

            var result = input.Preprocess();

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
        [Fact]
        public void Preprocess_WithNegativeConditionals_Stripped()
        {
            string input = "class SomeClass {\r\n" +
                           "#if !CONDITIONAL\r\n" +
                           "    void MyMethod1()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "#endif\r\n" +
                           "    void MyMethod2()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "}";

            string expected = "class SomeClass {\r\n" +
                              "    void MyMethod1()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "    void MyMethod2()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "}";

            var result = input.Preprocess();

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void Preprocess_WithNegativeConditionals_ConditionalSpecifed_NotStripped()
        {
            string input = "class SomeClass {\r\n" +
                           "#if !CONDITIONAL\r\n" +
                           "    void MyMethod1()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "#endif\r\n" +
                           "    void MyMethod2()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "}";

            string expected = "class SomeClass {\r\n" +
                              "    void MyMethod2()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "}";

            var result = input.Preprocess("CONDITIONAL");

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }


        [Fact]
        public void Preprocess_WithConditionals_DirectiveSpecified_Retained()
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

            string expected = "class SomeClass {\r\n" +
                              "    void MyMethod1()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "    void MyMethod2()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "}";

            var result = input.Preprocess("CONDITIONAL");

            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void Preprocess_IfElse()
        {
            var input = "#if FOO\r\n" +
                        "FOO\r\n" +
                        "#elif BAR\r\n" +
                        "BAR\r\n" +
                        "#else\r\n" +
                        "BAZ\r\n" +
                        "#endif";

            var result = input.Preprocess("FOO");
            var expected = "FOO";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }


        [Fact]
        public void Preprocess_IfElIf()
        {
            var input = "#if FOO\r\n" +
                        "FOO\r\n" +
                        "#elif BAR\r\n" +
                        "BAR\r\n" +
                        "#else\r\n" +
                        "BAZ\r\n" +
                        "#endif";

            var result = input.Preprocess("BAR");
            var expected = "BAR";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        [Fact]
        public void Preprocess_IfElse_2()
        {
            var input = "#if DEBUG\r\n" +
                        "DEBUG\r\n" +
                        "#else\r\n" +
                        "RELEASE\r\n" +
                        "#endif";

            var result = input.Preprocess("DEBUG");
            var expected = "DEBUG";
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }
    }
}