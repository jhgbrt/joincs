using System;
using System.IO;
using System.Linq;

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
            var inputDirectory = args[0];
            if (!Directory.Exists(inputDirectory))
            {
                Console.WriteLine("{0}: directory not found", inputDirectory);
                return 1;
            }
            var files = Directory.GetFiles(inputDirectory, "*.cs", SearchOption.AllDirectories);
            if (!files.Any())
            {
                Console.WriteLine("No .cs files found in folder {0}", inputDirectory);
                return 1;
            }

            try
            {
                var sources = files.Select(File.ReadAllText);

                var output = Joiner.Join(sources);

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
