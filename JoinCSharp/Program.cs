using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("JoinCSharp.UnitTests")]

namespace JoinCSharp
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var arguments = new Args(args);

            if (arguments.Errors.Any())
            {
                foreach (var error in arguments.Errors)
                {
                    await Console.Error.WriteLineAsync(error);
                }
                await Console.Out.WriteLineAsync(
                    "usage: joincs inputfolder [outputfile] [PREPROCESSOR_DIRECTIVE_1,PREPROCESSOR_DIRECTIVE_2,...]");
                return 1;
            }

            try
            {
                var inputDirectory = new DirectoryInfo(arguments.InputDirectory);

                var subdirs = new[] {"bin", "obj"}.Select(inputDirectory.SubFolder).ToArray();

                var output = inputDirectory
                    .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                    .Except(subdirs)
                    .WriteLine(Console.Out)
                    .ReadLines()
                    .Preprocess(arguments.PreprocessorDirectives)
                    .Aggregate();

                if (!string.IsNullOrEmpty(arguments.OutputFile))
                {
                    Console.WriteLine($"Writing result to {new FileInfo(arguments.OutputFile).FullName}");
                    await File.WriteAllTextAsync(arguments.OutputFile, output);
                }
                else
                {
                    await Console.Out.WriteAsync(output);
                }

                return 0;
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
                return 1;
            }
        }
    }
}
