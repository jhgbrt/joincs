using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JoinCSharp
{
    using static Extensions.State;
    internal static class Extensions
    {
        public static IEnumerable<FileInfo> Except(this IEnumerable<FileInfo> input, params DirectoryInfo[] folders) 
            => input.Where(file => !folders.Any(file.SitsBelow));

        public static DirectoryInfo SubFolder(this DirectoryInfo root, string sub) 
            => new(Path.Combine(root.FullName, sub));

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

        public static IEnumerable<string> ReadLines(this FileInfo file) 
            => File.ReadLines(file.FullName);

        public static IEnumerable<IEnumerable<string>> ReadLines(this IEnumerable<FileInfo> input) 
            => input.Select(ReadLines);

        public static string Aggregate(this IEnumerable<string> sources, bool includeAssemblyAttributes = false) 
            => sources.Aggregate(new SourceAggregator(includeAssemblyAttributes), (p, s) => p.AddSource(s)).GetResult();

        internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives) 
            => input.Select(x => string.Join(Environment.NewLine, x.Preprocess(directives)));

        internal enum State
        {
            OutsideIfDirective,
            SkippingIfDirective,
            KeepingIfDirective
        }

        static bool TryParseIfDirective(this ReadOnlySpan<char> line, out IfDirective result)
        {
            if (!line.StartsWith("#if "))
            {
                result = default;
                return false;
            }

            int index = line.IndexOf('!');
            bool not = index >= 0;
            if (!not)
            {
                index = 2;
            }

            string symbol = new(line[(index + 1)..].Trim());
            result = new(not, symbol);

            return !string.IsNullOrEmpty(symbol);
        }

        static bool IsEndIfDirective(this ReadOnlySpan<char> line) 
            => line.TrimStart().StartsWith("#endif");

        static bool IsElseDirective(this ReadOnlySpan<char> line) 
            => line.TrimStart().StartsWith("#else");

        record IfDirective(bool Not, string Symbol);

        internal static IEnumerable<string> Preprocess(this IEnumerable<string> input, params string[] directives)
        {
            var state = OutsideIfDirective;
            foreach (string line in input)
            {
                var span = line.AsSpan();
                switch (state)
                {
                    case OutsideIfDirective:
                        {
                            span = span.TrimStart();
                            if (span.TryParseIfDirective(out IfDirective result))
                            {
                                (bool not, string symbol) = result;
                                var codeShouldBeIncluded = directives.Any(directive => symbol == directive) ? !not : not;
                                state = codeShouldBeIncluded ? KeepingIfDirective : SkippingIfDirective;
                            }
                            else
                            {
                                yield return line;
                            }
                            break;
                        }
                    case KeepingIfDirective:
                        {
                            if (span.IsEndIfDirective())
                            {
                                state = OutsideIfDirective;
                            }
                            else if (span.IsElseDirective())
                            {
                                state = SkippingIfDirective;
                            }
                            else
                            {
                                yield return line;
                            }

                            break;
                        }
                    case SkippingIfDirective:
                        {
                            if (span.IsEndIfDirective())
                            {
                                state = OutsideIfDirective;
                            }
                            else if (span.IsElseDirective())
                            {
                                state = KeepingIfDirective;
                            }
                            break;
                        }
                }
            }
        }

        internal static IEnumerable<string> ReadLines(this string input)
        {
            using StringReader reader = new(input);
            while (reader.Peek() >= 0)
            {
                yield return reader.ReadLine();
            }
        }
    }
}
