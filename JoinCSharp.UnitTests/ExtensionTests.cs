using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JoinCSharp.UnitTests
{
    static class Ex
    {
        public static string Preprocess(this string s, params string[] directives)
        {
            return s.ReadLines().Preprocess(directives);
        }
    }
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void IsBelow_FileInFolder_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users\Joe");
            Assert.IsTrue(fileInfo.SitsBelow(root));
        }
        [TestMethod]
        public void IsBelow_FileInFolderBelow_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users");
            Assert.IsTrue(fileInfo.SitsBelow(root));
        }
        [TestMethod]
        public void IsBelow_FileInRootFolderBelow_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\");
            Assert.IsTrue(fileInfo.SitsBelow(root));
        }
        [TestMethod]
        public void IsBelow_FileOtherFolderBelow_False()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users\Jane");
            Assert.IsFalse(fileInfo.SitsBelow(root));
        }

        [TestMethod]
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

            CollectionAssert.AreEqual(expected, result.Select(f => f.FullName).ToArray());
        }

        [TestMethod]
        public void WriteLine_WritesNameToTextWriter()
        {
            var fileInfos = new[] {@"C:\A\B\C.txt"}.Select(s => new FileInfo(s));
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                fileInfos = fileInfos.WriteLine(writer).ToList();
            }
            Assert.AreEqual(@"Processing: C:\A\B\C.txt" + Environment.NewLine, sb.ToString());
        }

        [TestMethod]
        public void Preprocess_Empty_Remains()
        {
            Assert.AreEqual(string.Empty, string.Empty.Preprocess());
        }
        [TestMethod]
        public void Preprocess_OnlyConditional_BecomesEmpty()
        {
            Assert.AreEqual(string.Empty, "#if WHATEVER\r\n#endif".Preprocess());
        }
        [TestMethod]
        public void Preprocess_AdditionalWhitespace_BecomesEmpty()
        {
            Assert.AreEqual(string.Empty, "   #if   WHATEVER\t \r\n\t #endif".Preprocess());
        }
        [TestMethod]
        public void Preprocess_AdditionalWhitespace_Negative_BecomesEmpty()
        {
            Assert.AreEqual(string.Empty, "   #if   !WHATEVER\t \r\n\t #endif".Preprocess());
        }
        [TestMethod]
        public void Preprocess_NoWhitespace_Negative_BecomesEmpty()
        {
            Assert.AreEqual(string.Empty, "#if !WHATEVER\t \r\n\t #endif".Preprocess());
        }
        [TestMethod]
        public void Preprocess_InvalidDirective_NotTouched()
        {
            Assert.AreEqual("#if\r\n#endif", "#if\r\n#endif".Preprocess());
        }
        [TestMethod]
        public void Preprocess_InvalidNegativeDirective_NotTouched()
        {
            Assert.AreEqual("#if !\r\n#endif", "#if !\r\n#endif".Preprocess());
        }
        [TestMethod]
        public void Preprocess_InvalidNegativeDirective2_NotTouched()
        {
            Assert.AreEqual("#if!\r\n#endif", "#if!\r\n#endif".Preprocess());
        }

        [TestMethod]
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

            Assert.AreEqual(input, result);
        }
        [TestMethod]
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

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
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

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
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

            Assert.AreEqual(expected, result);
        }


        [TestMethod]
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

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Preprocess_IfElse()
        {
            var input = "#if DEBUG\r\n" +
                        "DEBUG\r\n" +
                        "#else\r\n" +
                        "RELEASE\r\n" +
                        "#endif";

            var result = input.Preprocess("RELEASE");

            Assert.AreEqual("RELEASE", result);
        }
        [TestMethod]
        public void Preprocess_IfElse_2()
        {
            var input = "#if DEBUG\r\n" +
                        "DEBUG\r\n" +
                        "#else\r\n" +
                        "RELEASE\r\n" +
                        "#endif";

            var result = input.Preprocess("DEBUG");

            Assert.AreEqual("DEBUG", result);
        }
    }
}