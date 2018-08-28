using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Linq;

[assembly: InternalsVisibleTo("JoinCSharp.UnitTests")]

namespace JoinCSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            var arguments = new Args(args);

            if (arguments.Errors.Any())
            {
                foreach (var error in arguments.Errors)
                {
                    Console.Error.WriteLine(error);
                }
                Console.WriteLine(
                    "usage: joincs inputfolder [outputfile] [PREPROCESSOR_DIRECTIVE_1,PREPROCESSOR_DIRECTIVE_2,...]");
                return 1;
            }

            var directoryInfo = new DirectoryInfo(arguments.InputDirectory);

            var files = directoryInfo.GetFiles("*.cs", SearchOption.AllDirectories)
                .Except(directoryInfo, "bin", "obj")
                .ToList();

            if (!files.Any())
            {
                Console.Error.WriteLine($"No .cs files found in folder {arguments.InputDirectory}");
                return 1;
            }

            try
            {
                var output = files.WriteLine(Console.Out).ReadContent().Join(arguments.PreprocessorDirectives);

                if (!string.IsNullOrEmpty(arguments.OutputFile))
                {
                    Console.WriteLine($"Writing result to {new FileInfo(arguments.OutputFile).FullName}");
                    File.WriteAllText(arguments.OutputFile, output);
                }
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
    }
}
