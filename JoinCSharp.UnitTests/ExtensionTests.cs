using System;
using System.IO;
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
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users\Joe");
            Assert.True(fileInfo.SitsBelow(root));
        }
        [Fact]
        public void IsBelow_FileInFolderBelow_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users");
            Assert.True(fileInfo.SitsBelow(root));
        }
        [Fact]
        public void IsBelow_FileInRootFolderBelow_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\");
            Assert.True(fileInfo.SitsBelow(root));
        }
        [Fact]
        public void IsBelow_FileOtherFolderBelow_False()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users\Jane");
            Assert.False(fileInfo.SitsBelow(root));
        }

        [Fact]
        public void Except_FiltersFileInSubfolders()
        {
            var input = new[]
            {
                @"C:\A\AA\AAA.txt",
                @"C:\A\AB\AAB.txt",
                @"C:\A\AC\AAC.txt",
                @"C:\A\AD\AAD.txt",
                @"C:\A\AD\AAE.txt",
                @"C:\A\AE\AAF.txt",
            }.Select(s => new FileInfo(s));

            var rootDir = new DirectoryInfo(@"C:\A");
            var subdirs = new[] { "AB", "AD"}.Select(rootDir.SubFolder).ToArray();

            var result = input.Except(subdirs);

            var expected = new[]
            {
                @"C:\A\AA\AAA.txt",
                @"C:\A\AC\AAC.txt",
                @"C:\A\AE\AAF.txt",
            };

            Assert.Equal(expected, result.Select(f => f.FullName).ToArray());
        }

        [Fact]
        public void Preprocess_Empty_Remains()
        {
            Assert.Equal(string.Empty, string.Empty.Preprocess());
        }
        [Fact]
        public void Preprocess_OnlyConditional_BecomesEmpty()
        {
            Assert.Equal(string.Empty, "#if WHATEVER\r\n#endif".Preprocess());
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_BecomesEmpty()
        {
            Assert.Equal(string.Empty, "   #if   WHATEVER\t \r\n\t #endif".Preprocess());
        }
        [Fact]
        public void Preprocess_AdditionalWhitespace_Negative_BecomesEmpty()
        {
            Assert.Equal(string.Empty, "   #if   !WHATEVER\t \r\n\t #endif".Preprocess());
        }
        [Fact]
        public void Preprocess_NoWhitespace_Negative_BecomesEmpty()
        {
            Assert.Equal(string.Empty, "#if !WHATEVER\t \r\n\t #endif".Preprocess());
        }
        [Fact]
        public void Preprocess_InvalidDirective_NotTouched()
        {
            Assert.Equal("#if\r\n#endif", "#if\r\n#endif".Preprocess());
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective_NotTouched()
        {
            Assert.Equal("#if !\r\n#endif", "#if !\r\n#endif".Preprocess());
        }
        [Fact]
        public void Preprocess_InvalidNegativeDirective2_NotTouched()
        {
            Assert.Equal("#if!\r\n#endif", "#if!\r\n#endif".Preprocess());
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

            Assert.Equal(input, result);
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

            Assert.Equal(expected, result);
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

            Assert.Equal(expected, result);
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

            Assert.Equal(expected, result);
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

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Preprocess_IfElse()
        {
            var input = "#if DEBUG\r\n" +
                        "DEBUG\r\n" +
                        "#else\r\n" +
                        "RELEASE\r\n" +
                        "#endif";

            var result = input.Preprocess("RELEASE");

            Assert.Equal("RELEASE", result);
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

            Assert.Equal("DEBUG", result);
        }
    }
}