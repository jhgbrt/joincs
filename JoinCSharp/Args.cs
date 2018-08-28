using System.Collections.Generic;
using System.IO;

namespace JoinCSharp
{
    class Args
    {
        public Args(string[] args)
        {

            foreach (var arg in args)
            {
                if (Directory.Exists(arg) && string.IsNullOrEmpty(InputDirectory))
                {
                    InputDirectory = arg;
                }
                else if (Path.HasExtension(arg) && string.IsNullOrEmpty(OutputFile))
                {
                    if (Path.GetExtension(arg) == ".cs")
                    {
                        OutputFile = arg;
                    }
                    else
                    {
                        Errors.Add($"Expected '.cs' as extension for output file, but was {Path.GetExtension(arg)}");
                    }
                }
                else
                {
                    PreprocessorDirectives = arg.Split(',');
                }
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
}