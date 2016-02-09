using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoinCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
                Console.WriteLine("usage: joincs inputfolder [outputfile]");

            var sources = Directory.GetFiles(args[0], "*.cs").Select(File.ReadAllText);
            
            var output = Joiner.Join(sources);
            
            if (args.Length == 2)
            {
                File.WriteAllText(output, args[1]);
            }
            else
            {
                Console.WriteLine(output);
            }
        }
    }
}
