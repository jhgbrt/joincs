using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JoinCSharp
{
    public static class Extensions
    {
        public static IEnumerable<FileInfo> Except(this IEnumerable<FileInfo> input, params DirectoryInfo[] folders) 
            => input.Where(file => !folders.Any(file.SitsBelow));

        public static DirectoryInfo SubFolder(this DirectoryInfo root, string sub) 
            => new DirectoryInfo(Path.Combine(root.FullName, sub));

        public static bool SitsBelow(this FileInfo file, DirectoryInfo folder) 
            => file.Directory.Parents().Any(dir => dir.FullName.Equals(folder.FullName));

        public static IEnumerable<DirectoryInfo> Parents(this DirectoryInfo info)
        {
            var item = info;
            while (item != null)
            {
                yield return item;
                item = item.Parent;
            }
        }
        
        public static IEnumerable<FileInfo> WriteLine(this IEnumerable<FileInfo> input, TextWriter writer)
        {
            foreach (var f in input)
            {
                writer.WriteLine($"Processing: {f.FullName}");
                yield return f;
            }
        }

        public static string ReadAllText(this FileInfo file)
            => File.ReadAllText(file.FullName);

        public static IEnumerable<string> ReadAllText(this IEnumerable<FileInfo> input) 
            => input.Select(ReadAllText);

        public static string Aggregate(this IEnumerable<string> sources, params string[] preprocessorSymbols)
            => sources.Aggregate(new SourceAggregator(preprocessorSymbols?.ToArray()), (p, s) => p.AddSource(s)).GetResult();

        internal static IEnumerable<T> WithoutTrivia<T>(this IEnumerable<T> input) where T : SyntaxNode 
            => input.Select(x => x.WithoutTrivia());
    }
}
