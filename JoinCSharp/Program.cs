using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: InternalsVisibleTo("JoinCSharp.UnitTests")]

namespace JoinCSharp
{
    class Args
    {
        public Args(string[] args)
        {

            foreach (var arg in args)
            {
                if (Directory.Exists(arg) && string.IsNullOrEmpty(InputDirectory))
                    InputDirectory = arg;
                else if (Path.HasExtension(arg) && string.IsNullOrEmpty(OutputFile))
                {
                    if (Path.GetExtension(arg) == ".cs")
                        OutputFile = arg;
                    else
                        Errors.Add($"Expected '.cs' as extension for output file, but was {Path.GetExtension(arg)}");
                }
                else
                    PreprocessorDirectives = arg.Split(',');
            }

            if (args.Length < 1 || args.Length > 3)
            {
                Errors.Add("Wrong nof arguments");
            }
            else if (!Directory.Exists(InputDirectory))
            {
                Errors.Add($"{InputDirectory}: directory not found");
            }
        }

        public string InputDirectory { get; }
        public string OutputFile { get; }
        public string[] PreprocessorDirectives { get; }
        public List<string> Errors { get; } = new List<string>();
    }

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

            var files = Directory.GetFiles(arguments.InputDirectory, "*.cs", SearchOption.AllDirectories);
            if (!files.Any())
            {
                Console.Error.WriteLine($"No .cs files found in folder {arguments.InputDirectory}");
                return 1;
            }

            try
            {
                var sources = files.Select(File.ReadAllText);

                var output = sources.Join(arguments.PreprocessorDirectives);

                if (string.IsNullOrEmpty(arguments.OutputFile))
                    File.WriteAllText(arguments.OutputFile, output);
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
