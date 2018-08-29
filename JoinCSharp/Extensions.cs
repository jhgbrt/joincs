using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public static IEnumerable<string> ReadLines(this FileInfo file)
            => File.ReadLines(file.FullName);

        public static IEnumerable<IEnumerable<string>> ReadLines(this IEnumerable<FileInfo> input) 
            => input.Select(ReadLines);

        public static string Aggregate(this IEnumerable<string> sources)
            => sources.Aggregate(new SourceAggregator(), (p, s) => p.AddSource(s)).GetResult();

        internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives)
            => input.Select(x => x.Preprocess(directives));

        enum State
        {
            OutsideIfDirective,
            SkippingIfDirective,
            KeepingIfDirective
        }

        internal static bool ParseDirective(this ReadOnlySpan<char> line, out (bool not, string symbol) result)
        {
            result = (false, string.Empty);
            if (!line.StartsWith("#if "))
                return false;

            var index = line.IndexOf('!');
            var not = index >= 0;
            if (!not) index = 2;
            var symbol = new string(line.Slice(index+1).Trim());
            result = (not, symbol);

            return !string.IsNullOrEmpty(symbol);
        }

        internal static bool IsEndIfDirective(this ReadOnlySpan<char> line)
            => line.TrimStart().StartsWith("#endif");

        internal static string Preprocess(this IEnumerable<string> input, params string[] directives)
        {
            var sb = new StringBuilder();
            var state = State.OutsideIfDirective;
            foreach (var line in input)
            {
                switch (state)
                {
                    case State.OutsideIfDirective:
                    {
                        var span = line.AsSpan().TrimStart();
                        if (span.ParseDirective(out var result))
                        {
                            var (not,symbol) = result;
                            if (directives.Any(directive => symbol == directive))
                            {
                                state = not ? State.SkippingIfDirective: State.KeepingIfDirective;
                            }
                            else
                            {
                                state = not ? State.KeepingIfDirective : State.SkippingIfDirective;
                            }
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
            using (var reader = new StringReader(input))
            {
                while (reader.Peek() >= 0)
                    yield return reader.ReadLine();
            }
        }
    }
}
