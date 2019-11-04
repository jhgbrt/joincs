using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace JoinCSharp.UnitTests
{
    [TestClass]
    public class SourceAggregatorTests
    {
        private static string Process(string input, params string[] preprocessorSymbols)
        {
            return new SourceAggregator(false).AddSource(input.Preprocess(preprocessorSymbols)).GetResult();
        }

        [TestMethod]
        public void SimpleEnum()
        {
            var input = "enum MyEnum {A, B, C}";

            var result = Process(input);

            var expected = "enum MyEnum\r\n" +
                            "{\r\n" +
                            "    A,\r\n" +
                            "    B,\r\n" +
                            "    C\r\n" +
                            "}";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SimpleClass()
        {
            var input = "class SomeClass {}";

            var result = Process(input);

            var expected = "class SomeClass\r\n" +
                            "{\r\n" +
                            "}";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ExternAlias()
        {
            var input = "extern alias SomeAlias;";

            var result = Process(input);

            var expected = "extern alias SomeAlias;";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AssemblyAttribute()
        {
            var input = "using SomeNamespace;\r\n[assembly: SomeAttribute()]";

            var result = Process(input);

            var expected = "using SomeNamespace;\r\n\r\n[assembly: SomeAttribute()]";

            Assert.AreEqual(expected, result);
        }
        
        [TestMethod]
        public void AssemblyAttributeList()
        {
            var input = "using SomeNamespace;\r\n[assembly: SomeAttribute(), MyAttribute()]";

            var result = Process(input);

            var expected = "using SomeNamespace;\r\n\r\n[assembly: MyAttribute(), SomeAttribute()]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MultipleAssemblyAttributes()
        {
            var input = new[]
            {
                "using SomeNamespace;\r\n" +
                "[assembly: SomeAttribute2()]\r\n",
                "[assembly: SomeAttribute1()]"
            };

            var result = input.Aggregate();

            var expected = "using SomeNamespace;\r\n" +
                           "\r\n" +
                           "[assembly: SomeAttribute1(), SomeAttribute2()]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IgnoreAssemblyAttributes()
        {
            var input = new[]
            {
                "using SomeNamespace;\r\n" +
                "[assembly: SomeAttribute2()]\r\n",
                "[assembly: SomeAttribute1()]"
            };

            var result = input.Aggregate(true);

            var expected = "using SomeNamespace;";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ClassInNamespaceWithUsing()
        {
            var input = "using Some.Using; namespace Some.Namespace { class SomeClass {} }";

            var result = Process(input);

            var expected = "using Some.Using;\r\n" +
                           "\r\n" +
                           "namespace Some.Namespace\r\n" +
                           "{\r\n" +
                           "    class SomeClass\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "}";

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void TwoSameUsingsAreGrouped()
        {
            var input = "using MyUsing;\r\nusing MyUsing;";
            var result = Process(input);
            Assert.AreEqual("using MyUsing;", result);
        }
        [TestMethod]
        public void TwoDifferentUsingsAreOrdered()
        {
            var input = "using MyUsing2;\r\nusing MyUsing1;";
            var result = Process(input);
            Assert.AreEqual("using MyUsing1;\r\nusing MyUsing2;", result);
        }
        [TestMethod]
        public void StaticUsingInNamespace()
        {
            var input = "namespace Some.Namespace { using static SomeClass; }";

            var result = Process(input);

            var expected = "namespace Some.Namespace\r\n" +
                "{\r\n" +
                "    using static SomeClass;\r\n" +
                "}";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConditionalIsStripped()
        {
            var input = "#if CONDITIONAL\r\nusing MyUsing;\r\n#endif";
            var result = Process(input, "CONDITIONAL");
            Assert.AreEqual("using MyUsing;", result);
        }

        [TestMethod]
        public void ConditionalIsStrippedFromCode()
        {
            var input = "using MyUsing1;\r\n#if CONDITIONAL\r\nusing MyUsing;\r\n#endif";
            var result = Process(input, "CONDITIONAL");
            Assert.AreEqual("using MyUsing;\r\nusing MyUsing1;", result);
        }


        [TestMethod]
        public void WhenCompilingWithPreprocessorDirective_ConditionalCodeIsRetained()
        {
            var input =
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n" +
                "}";

            var result = Process(input, new[] { "CONDITIONAL" });

            var expected = "namespace Abc.Def\r\n" +
                "{\r\n" +
                "    class ConditionalClass\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "}";

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void SimpleUsing()
        {
            string input = "using Some.Using;";
            string expected = "using Some.Using;";

            var result = Process(input, new string[] { "CONDITIONAL" });

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConditionalUsing_NoPreprocessorSymbols_UsingIsRemoved()
        {
            string input = "#if CONDITIONAL\r\n" +
                "using Some.Using;\r\n" +
                "#endif";

            var result = Process(input, new string[] { });

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ConditionalUsing_WithPreprocessorSymbol_UsingIsMaintained()
        {
            string input = "#if CONDITIONAL\r\n" +
                "using Some.Using;\r\n" +
                "#endif";

            string expected = "using Some.Using;";

            var result = Process(input, "CONDITIONAL");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WhenCompilingWithoutConditionalDirective_ConditionalCodeIsStrippedAway()
        {
            var input =
                "#if CONDITIONAL\r\n" +
                "using Some.ConditionalUsing;\r\n" +
                "#endif\r\n" +
                "using Some.Using1;\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n" +
                "}";

            var result = Process(input);

            var expected =
                "using Some.Using1;\r\n\r\n" +
                "namespace Abc.Def\r\n" +
                "{\r\n" +
                "}";

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void ProcessUsings()
        {
            string input = "using MyUsing1;\r\nusing MyUsing2;";

            string result = Process(input);

            Assert.AreEqual("using MyUsing1;\r\nusing MyUsing2;", result);
        }

        [TestMethod]
        public void ConditionalIsNotStrippedFromCode()
        {
            string input =
        "using MyUsing1;\r\n" +
        "#if CONDITIONAL\r\n" +
        "using MyUsing2;\r\n" +
        "#endif";
            string result = Process(input, "CONDITIONAL");
            Assert.AreEqual("using MyUsing1;\r\nusing MyUsing2;", result);
        }

        // TODO investigate
        //[TestMethod]
        //[Ignore("not supported (yet)")]
        public void ClassWithComment()
        {
            var input = "// some comment\r\nclass SomeClass {}";

            var result = Process(input);

            var expected = "// some comment\r\n" +
                           "class SomeClass\r\n" +
                           "{\r\n" +
                           "}";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConditionalMethod_NoSymbols_MethodIsStripped()
        {
            string input = "class SomeClass {\r\n" +
                "#if CONDITIONAL\r\n" +
                "    void MyMethod()\r\n" +
                "    {\r\n" +
                "    }\r\n" +
                "#endif\r\n" +
                "}";

            string expected = "class SomeClass\r\n" +
                            "{\r\n" +
                            "}";

            var result = Process(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConditionalMethod_WithSymbols_MethodIsNotStripped()
        {
            string input = "class SomeClass {\r\n" +
                           "#if CONDITIONAL1\r\n" +
                           "    void MyMethod1()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "#endif\r\n" +
                           "#if CONDITIONAL2\r\n" +
                           "    void MyMethod2()\r\n" +
                           "    {\r\n" +
                           "    }\r\n" +
                           "#endif\r\n" +
                           "}";

            string expected = "class SomeClass\r\n" +
                              "{\r\n" +
                              "    void MyMethod2()\r\n" +
                              "    {\r\n" +
                              "    }\r\n" +
                              "}";

            var result = Process(input, "CONDITIONAL2");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConditionalClass_WithDirective_ClassIsStripped()
        {
            string input = "namespace Abc.Def\r\n" +
                "{\r\n" +
                "#if CONDITIONAL\r\n" +
                "   class ConditionalClass{}\r\n" +
                "#endif\r\n" +
                "}";

            string expected = "namespace Abc.Def\r\n" +
                            "{\r\n" +
                            "    class ConditionalClass\r\n" +
                            "    {\r\n" +
                            "    }\r\n" +
                            "}";

            var result = Process(input, new string[] { "CONDITIONAL" });

            Assert.AreEqual(expected, result);
        }


    }
}
