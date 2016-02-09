using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JoinCSharp.UnitTests
{
    [TestClass]
    public class InMemoryJoinTests
    {
        [TestMethod]
        public void JoinTest()
        {
            var sources = new[]
            {
                "using Some.Using1;" +
                "using Some.Using2;\r\n" +
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
            };

            var result = Joiner.Join(sources);
            var expected = @"using Some.Using1;
using Some.Using2;
using Some.Using3;

namespace Abc.Def
{
    // comment 
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
            //Process.Start("result.txt");
            Assert.AreEqual(expected, result);
        }
    }
}
