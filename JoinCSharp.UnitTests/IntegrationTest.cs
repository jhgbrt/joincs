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

            string expectedWithoutConditional = @"using Some.Using1;
using Some.Using2;
using Some.Using3;

namespace Abc.Def
{
    class A
    {
    }

    class B
    {
        public void y()
        {
            DomeSomething();
        }
    }
}

namespace Abc.Def.Ghi
{
    class C
    {
        public void x()
        { // IDbConnection sql;
            DomeSomething2();
        }
    }
}

namespace Xkcd.WhatIf
{
    class G
    {
    }
}

namespace Xyz.Vwu
{
    class E
    {
    }
}

class C
{
    public static dynamic x()
    {
        return null;
    }
}";
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
            string expectedWithoutConditional = @"using Some.Using1;
using Some.Using2;
using Some.ConditionalUsing;
using Some.Using3;

namespace Abc.Def
{
    class A
    {
    }

    class B
    {
        public void y()
        {
            DomeSomething();
        }
    }

    class ConditionalClass
    {
    }
}

namespace Abc.Def.Ghi
{
    class C
    {
        public void x()
        { // IDbConnection sql;
            DomeSomething2();
        }
    }
}

namespace Xkcd.WhatIf
{
    class G
    {
    }
}

namespace Xyz.Vwu
{
    class E
    {
    }
}

class C
{
    public static dynamic x()
    {
        return null;
    }
}";
            //File.WriteAllText("result.txt", result);
            //File.WriteAllText("expected.txt", expectedWithoutConditional);
            //Process.Start(@"c:\Program Files (x86)\WinMerge\WinMergeU.exe", "result.txt expected.txt");
            Assert.AreEqual(expectedWithoutConditional, result);
        }

    }
}
