using System.Diagnostics;

using Xunit;

namespace JoinCSharp.UnitTests;

public class IntegrationTest
{
    private static readonly string[] sources =
    [
            "using Some.Using1;" +
            "using Some.Using2;" + Environment.NewLine + "" +
            "#if CONDITIONAL" + Environment.NewLine + "" +
            "using Some.ConditionalUsing;" + Environment.NewLine + "" +
            "#endif" + Environment.NewLine + "" +
            "using static Abc.StaticClass;" + Environment.NewLine + "" + 
            "namespace Abc.Def " +
            "{ " + Environment.NewLine + "" +
            "using static StaticClass;" + Environment.NewLine + "" +
            "// comment" + Environment.NewLine + "" +
            "class A {} }",

            "using Some.Using1; using Some.Using3; namespace Abc.Def { class B {   public void y() { DomeSomething(); }} }",

            "using Some.Using1; namespace Abc.Def.Ghi { class C {    public void x() {" +
            "       // IDbConnection sql;" + Environment.NewLine + "" +
            "       DomeSomething2();        }    } }",

            "using Some.Using3; namespace Xyz.Vwu { class E {} } namespace Xkcd.WhatIf { class G {} }",

            "using Some.Using1; class C { " + Environment.NewLine + "" +
            "" + Environment.NewLine + "// commentx" + Environment.NewLine + "" +
            "public static dynamic x() {return null;} } ",

            "#if CONDITIONAL" + Environment.NewLine + "" +
            "using Some.ConditionalUsing;" + Environment.NewLine + "" +
            "#endif" + Environment.NewLine + "" +
            "using Some.Using1;" + Environment.NewLine + "" +
            "namespace Abc.Def" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "#if CONDITIONAL" + Environment.NewLine + "" +
            "   class ConditionalClass{}" + Environment.NewLine + "" +
            "#endif" + Environment.NewLine + ""+
            "}"
        ];

    [Fact]
    public void JoinTest_WithoutPreprocessorDirective()
    {

        string result = sources.Select(s => s.ReadLines()).Preprocess().Aggregate();

        string expected =
            "using static Abc.StaticClass;" + Environment.NewLine + "" +
            "using Some.Using1;" + Environment.NewLine + "" +
            "using Some.Using2;" + Environment.NewLine + "" +
            "using Some.Using3;" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Abc.Def" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    using static StaticClass;" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "    // comment" + Environment.NewLine + "" +
            "    class A { }" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "    class B" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "        public void y()" + Environment.NewLine + "" +
            "        {" + Environment.NewLine + "" +
            "            DomeSomething();" + Environment.NewLine + "" +
            "        }" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "\r\nnamespace Abc.Def.Ghi" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    class C" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "        public void x()" + Environment.NewLine + "" +
            "        { // IDbConnection sql;" + Environment.NewLine + "" +
            "            DomeSomething2();" + Environment.NewLine + "" +
            "        }" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Xkcd.WhatIf" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    class G { }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Xyz.Vwu" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    class E { }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "\r\nclass C" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    // commentx" + Environment.NewLine + "" +
            "    public static dynamic x()" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "        return null;" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "}";

        ShowInteractiveDiffIfDifferent(result, expected);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void JoinTest_WithPreprocessorDirective()
    {

        string result = sources.Select(s => s.ReadLines()).Preprocess("CONDITIONAL").Aggregate();

        string expected =
            "using static Abc.StaticClass;" + Environment.NewLine + "" +
            "using Some.ConditionalUsing;" + Environment.NewLine + "" +
            "using Some.Using1;" + Environment.NewLine + "" +
            "using Some.Using2;" + Environment.NewLine + "" +
            "using Some.Using3;" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Abc.Def" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    using static StaticClass;" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "    // comment" + Environment.NewLine + "" +
            "    class A { }" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "    class B" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "        public void y()" + Environment.NewLine + "" +
            "        {" + Environment.NewLine + "" +
            "            DomeSomething();" + Environment.NewLine + "" +
            "        }" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "    class ConditionalClass { }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Abc.Def.Ghi" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    class C" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "        public void x()" + Environment.NewLine + "" +
            "        { // IDbConnection sql;" + Environment.NewLine + "" +
            "            DomeSomething2();" + Environment.NewLine + "" +
            "        }" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Xkcd.WhatIf" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    class G { }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "namespace Xyz.Vwu" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    class E { }" + Environment.NewLine + "" +
            "}" + Environment.NewLine + "" +
            "" + Environment.NewLine + "" +
            "class C" + Environment.NewLine + "" +
            "{" + Environment.NewLine + "" +
            "    // commentx" + Environment.NewLine + "" +
            "    public static dynamic x()" + Environment.NewLine + "" +
            "    {" + Environment.NewLine + "" +
            "        return null;" + Environment.NewLine + "" +
            "    }" + Environment.NewLine + "" +
            "}";

        ShowInteractiveDiffIfDifferent(result, expected);
        Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
    }

    static readonly Lazy<string> winmerge = new(() => new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WinMerge", "WinMergeU.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WinMerge", "WinMergeU.exe"),
            "WinMergeU.exe"
        }.FirstOrDefault(File.Exists));

    private static void ShowInteractiveDiffIfDifferent(string result, string expected)
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (!Environment.MachineName.StartsWith("DESKTOP"))
            return;

        if (winmerge.Value is null)
            return;

        if (!string.IsNullOrEmpty(winmerge.Value) && result != expected)
        {
            File.WriteAllText("result.txt", result);
            File.WriteAllText("expected.txt", expected);
            Process.Start(winmerge.Value, "result.txt expected.txt");
        }
    }

}
