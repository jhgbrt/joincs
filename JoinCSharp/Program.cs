using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace JoinCSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("usage: joincs inputfolder [outputfile]");
                return 1;
            }

            try
            {
                var sources = Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText);

                var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

                var output = Joiner.Join(syntaxTrees);

                if (args.Length == 2)
                {
                    File.WriteAllText(args[1], output);
                }
                else
                {
                    Console.WriteLine(output);
                }
                return 0;

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
