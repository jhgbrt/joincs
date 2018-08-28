using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JoinCSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("usage: joincs inputfolder [outputfile] [PREPROCESSOR_DIRECTIVE_1,PREPROCESSOR_DIRECTIVE_2,...]");
                return 1;
            }
            var inputDirectory = args[0];
            if (!Directory.Exists(inputDirectory))
            {
                Console.WriteLine("{0}: directory not found", inputDirectory);
                return 1;
            }

            var (outputFile, preprocessorSymbols) = ProcessArguments(args.Skip(1));

            var preprocessorDirectives = args.Length > 2 ? args[2].Split(',') : new string[0];

            var files = Directory.GetFiles(inputDirectory, "*.cs", SearchOption.AllDirectories);
            if (!files.Any())
            {
                Console.WriteLine("No .cs files found in folder {0}", inputDirectory);
                return 1;
            }

            try
            {
                var sources = files.Select(File.ReadAllText);

                var output = sources.Join(preprocessorDirectives);

                if (string.IsNullOrEmpty(outputFile))
                    File.WriteAllText(outputFile, output);
                else
                    Console.Write(output);

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }

        private static (string outputFile, string[] preprocessorSymbols) ProcessArguments(IEnumerable<string> args)
        {
            string outputFile = string.Empty;
            string[] preprocessorSymbols = new string[0];
            foreach (var arg in args)
            {
                if (Path.GetExtension(arg) == "cs")
                    outputFile = arg;
                else
                    preprocessorSymbols = arg.Split(',');
            }
            return (outputFile, preprocessorSymbols);
        }
    }
}
