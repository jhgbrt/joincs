using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JoinCSharp.UnitTests
{
    [TestClass]
    public class IntegrationTest
    {
        private static readonly string[] sources = new[]
{
                "using Some.Using1;" +
                "using Some.Using2;\r\n" +
                "#if CONDITIONAL\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "#endif\r\n" +
                "namespace Abc.Def " +
                "{ \r\n" +
                "// comment \r\n" +
                "class A {} }",

                "using Some.Using1; using Some.Using3; namespace Abc.Def { class B {   public void y() { DomeSomething(); }} }",

                "using Some.Using1; namespace Abc.Def.Ghi { class C {    public void x() {" +
                "       // IDbConnection sql;\r\n" +
                "       DomeSomething2();        }    } }",

                "using Some.Using3; namespace Xyz.Vwu { class E {} } namespace Xkcd.WhatIf { class G {} }",

                "using Some.Using1; class C { public static dynamic x() {return null;} } ",

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

        [TestMethod]
        public void JoinTest_WithoutPreprocessorDirective()
        {

            string result = sources.Join();

            string expectedWithoutConditional =
                "using Some.Using1;\r\n" +
                "using Some.Using2;\r\n" +
                "using Some.Using3;\r\n" +
                "\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n    class A\r\n" +
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
                "    public static dynamic x()\r\n" +
                "    {\r\n" +
                "        return null;\r\n" +
                "    }\r\n" +
                "}";
            //File.WriteAllText("result.txt", result);
            //File.WriteAllText("expected.txt", expectedWithoutConditional);
            //Process.Start(@"c:\Program Files (x86)\WinMerge\WinMergeU.exe", "result.txt expected.txt");
            Assert.AreEqual(expectedWithoutConditional, result);
        }

        [TestMethod]
        public void JoinTest_WithPreprocessorDirective()
        {

            string result = sources.Join("CONDITIONAL");

            // TODO class comments are stripped
            string expectedWithoutConditional =
                "using Some.Using1;\r\n" +
                "using Some.Using2;\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "using Some.Using3;\r\n" +
                "\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
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
                "    public static dynamic x()\r\n" +
                "    {\r\n" +
                "        return null;\r\n" +
                "    }\r\n" +
                "}";
            //File.WriteAllText("result.txt", result);
            //File.WriteAllText("expected.txt", expectedWithoutConditional);
            //Process.Start(@"c:\Program Files (x86)\WinMerge\WinMergeU.exe", "result.txt expected.txt");
            Assert.AreEqual(expectedWithoutConditional, result);
        }

    }
}
