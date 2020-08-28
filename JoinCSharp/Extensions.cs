using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JoinCSharp
{
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
            => input.Select(x => x.Preprocess(directives));

        private enum State
        {
            OutsideIfDirective,
            SkippingIfDirective,
            KeepingIfDirective
        }

        static bool ParseDirective(this ReadOnlySpan<char> line, out (bool not, string symbol) result)
        {
            result = (false, string.Empty);
            if (!line.StartsWith("#if "))
            {
                return false;
            }

            int index = line.IndexOf('!');
            bool not = index >= 0;
            if (!not)
            {
                index = 2;
            }

            string symbol = new(line[(index + 1)..].Trim());
            result = (not, symbol);

            return !string.IsNullOrEmpty(symbol);
        }

        static bool IsEndIfDirective(this ReadOnlySpan<char> line) 
            => line.TrimStart().StartsWith("#endif");

        static bool IsElseDirective(this ReadOnlySpan<char> line) 
            => line.TrimStart().StartsWith("#else");

        internal static string Preprocess(this IEnumerable<string> input, params string[] directives)
        {
            var sb = new StringBuilder();
            var state = State.OutsideIfDirective;
            foreach (string line in input)
            {
                switch (state)
                {
                    case State.OutsideIfDirective:
                        {
                            var span = line.AsSpan().TrimStart();
                            if (span.ParseDirective(out (bool, string) result))
                            {
                                (bool not, string symbol) = result;
                                var codeShouldBeIncluded = directives.Any(directive => symbol == directive) ? !not : not;
                                state = codeShouldBeIncluded ? State.KeepingIfDirective: State.SkippingIfDirective;
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }
                            break;
                        }
                    case State.KeepingIfDirective:
                        {
                            if (line.AsSpan().IsEndIfDirective())
                            {
                                state = State.OutsideIfDirective;
                            }
                            else if (line.AsSpan().IsElseDirective())
                            {
                                state = State.SkippingIfDirective;
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }

                            break;
                        }
                    case State.SkippingIfDirective:
                        {
                            if (line.AsSpan().IsEndIfDirective())
                            {
                                state = State.OutsideIfDirective;
                            }
                            else if (line.AsSpan().IsElseDirective())
                            {
                                state = State.KeepingIfDirective;
                            }
                            break;
                        }
                }
            }

            if (sb.Length > Environment.NewLine.Length)
            {
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            }
            return sb.ToString();
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
