using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace JoinCSharp.UnitTests
{
    public class IntegrationTest
    {
        private static readonly string[] sources = new[]
{
                "using Some.Using1;" +
                "using Some.Using2;\r\n" +
                "#if CONDITIONAL\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "#endif\r\n" +
                "using static Abc.StaticClass;\r\n" + 
                "namespace Abc.Def " +
                "{ \r\n" +
                "using static StaticClass;\r\n" +
                "// comment\r\n" +
                "class A {} }",

                "using Some.Using1; using Some.Using3; namespace Abc.Def { class B {   public void y() { DomeSomething(); }} }",

                "using Some.Using1; namespace Abc.Def.Ghi { class C {    public void x() {" +
                "       // IDbConnection sql;\r\n" +
                "       DomeSomething2();        }    } }",

                "using Some.Using3; namespace Xyz.Vwu { class E {} } namespace Xkcd.WhatIf { class G {} }",

                "using Some.Using1; class C { \r\n" +
                "\r\n// commentx\r\n" +
                "public static dynamic x() {return null;} } ",

                "#if CONDITIONAL\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "#endif\r\n" +
                "using Some.Using1;\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n"+
                "}"
            };

        [Fact]
        public void JoinTest_WithoutPreprocessorDirective()
        {

            string result = sources.Select(s => s.ReadLines()).Preprocess().Aggregate();

            string expected =
                "using static Abc.StaticClass;\r\n" +
                "using Some.Using1;\r\n" +
                "using Some.Using2;\r\n" +
                "using Some.Using3;\r\n" +
                "\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "    using static StaticClass;\r\n" +
                "\r\n" +
                "    // comment\r\n" +
                "    class A\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "\r\n" +
                "    class B\r\n" +
                "    {\r\n" +
                "        public void y()\r\n" +
                "        {\r\n" +
                "            DomeSomething();\r\n" +
                "        }\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\nnamespace Abc.Def.Ghi\r\n" +
                "{\r\n" +
                "    class C\r\n" +
                "    {\r\n" +
                "        public void x()\r\n" +
                "        { // IDbConnection sql;\r\n" +
                "            DomeSomething2();\r\n" +
                "        }\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\n" +
                "namespace Xkcd.WhatIf\r\n" +
                "{\r\n" +
                "    class G\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\n" +
                "namespace Xyz.Vwu\r\n" +
                "{\r\n" +
                "    class E\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\nclass C\r\n" +
                "{\r\n" +
                "    // commentx\r\n" +
                "    public static dynamic x()\r\n" +
                "    {\r\n" +
                "        return null;\r\n" +
                "    }\r\n" +
                "}";

            ShowInteractiveDiffIfDifferent(result, expected);
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }


        [Fact]
        public void JoinTest_WithPreprocessorDirective()
        {

            string result = sources.Select(s => s.ReadLines()).Preprocess("CONDITIONAL").Aggregate();

            string expected =
                "using static Abc.StaticClass;\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "using Some.Using1;\r\n" +
                "using Some.Using2;\r\n" +
                "using Some.Using3;\r\n" +
                "\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "    using static StaticClass;\r\n" +
                "\r\n" +
                "    // comment\r\n" +
                "    class A\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "\r\n" +
                "    class B\r\n" +
                "    {\r\n" +
                "        public void y()\r\n" +
                "        {\r\n" +
                "            DomeSomething();\r\n" +
                "        }\r\n" +
                "    }\r\n" +
                "\r\n" +
                "    class ConditionalClass\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\n" +
                "namespace Abc.Def.Ghi\r\n" +
                "{\r\n" +
                "    class C\r\n" +
                "    {\r\n" +
                "        public void x()\r\n" +
                "        { // IDbConnection sql;\r\n" +
                "            DomeSomething2();\r\n" +
                "        }\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\n" +
                "namespace Xkcd.WhatIf\r\n" +
                "{\r\n" +
                "    class G\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\n" +
                "namespace Xyz.Vwu\r\n" +
                "{\r\n" +
                "    class E\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}\r\n" +
                "\r\n" +
                "class C\r\n" +
                "{\r\n" +
                "    // commentx\r\n" +
                "    public static dynamic x()\r\n" +
                "    {\r\n" +
                "        return null;\r\n" +
                "    }\r\n" +
                "}";

            ShowInteractiveDiffIfDifferent(result, expected);
            Assert.Equal(expected.HandleCrLf(), result.HandleCrLf());
        }

        static Lazy<string> winmerge = new Lazy<string>(() => new[]
            {
                @"c:\Program Files\WinMerge\WinMergeU.exe",
                @"c:\Program Files (x 86)\WinMerge\WinMergeU.exe",
                "WinMergeU.exe"
            }.FirstOrDefault(File.Exists));

        private void ShowInteractiveDiffIfDifferent(string result, string expected)
        {
            if (!Environment.MachineName.StartsWith("DESKTOP"))
                return;

            if (!string.IsNullOrEmpty(winmerge.Value) && result != expected)
            {
                File.WriteAllText("result.txt", result);
                File.WriteAllText("expected.txt", expected);
                Process.Start(winmerge.Value, "result.txt expected.txt");
            }
        }

    }
}
