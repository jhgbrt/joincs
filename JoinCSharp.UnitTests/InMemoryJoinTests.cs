using System;
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
                "using System; using System.Text;\r\n\r\n// comment\r\n namespace Abc.Def { \r\n// comment \r\n class A {} }",
                "using System; " +
                "using System.IO;" +
                "namespace Abc.Def { class B {" +
                "   public void y() { File.ReadAllText(string.Empty); }" +
                "} }",
                "using System; using System.IO; namespace CD {" +
                " class C { " +
                "   public void x() { " +
                "       IDbConnection sql;" +
                "       File.ReadAllText(string.Empty); " +
                "       } " +
                "   } " +
                "}",
                "using System; namespace EF { class E {} } namespace GH { class G {} }",
                "using System; class C { public static dynamic x() {return null;} } ",
            };

            var result = Joiner.Join(sources);

            Console.WriteLine(result);
        }
    }
}
