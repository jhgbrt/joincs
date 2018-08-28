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

            var directoryInfo = new DirectoryInfo(arguments.InputDirectory);

            var files = directoryInfo.GetFiles("*.cs", SearchOption.AllDirectories)
                .Where(f => !f.FullName.StartsWith(Path.Combine(directoryInfo.FullName, "bin"), StringComparison.CurrentCultureIgnoreCase))
                .Where(f => !f.FullName.StartsWith(Path.Combine(directoryInfo.FullName, "obj"), StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (!files.Any())
            {
                Console.Error.WriteLine($"No .cs files found in folder {arguments.InputDirectory}");
                return 1;
            }

            try
            {
                var output = files.WriteLine().ReadContent().Join(arguments.PreprocessorDirectives);

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
