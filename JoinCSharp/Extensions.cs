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
        {
            return input.Where(file => !folders.Any(file.SitsBelow));
        }

        public static DirectoryInfo SubFolder(this DirectoryInfo root, string sub)
        {
            return new DirectoryInfo(Path.Combine(root.FullName, sub));
        }

        public static bool SitsBelow(this FileInfo file, DirectoryInfo folder)
        {
            return file.Directory.Parents().Any(dir => dir.FullName.Equals(folder.FullName));
        }

        public static IEnumerable<DirectoryInfo> Parents(this DirectoryInfo info)
        {
            DirectoryInfo item = info;
            while (item != null)
            {
                yield return item;
                item = item.Parent;
            }
        }

        public static IEnumerable<FileInfo> WriteLine(this IEnumerable<FileInfo> input, TextWriter writer)
        {
            foreach (FileInfo f in input)
            {
                writer.WriteLine($"Processing: {f.FullName}");
                yield return f;
            }
        }

        public static IEnumerable<string> ReadLines(this FileInfo file)
        {
            return File.ReadLines(file.FullName);
        }

        public static IEnumerable<IEnumerable<string>> ReadLines(this IEnumerable<FileInfo> input)
        {
            return input.Select(ReadLines);
        }

        public static string Aggregate(this IEnumerable<string> sources, bool ignoreAssemblyAttributeLists = false)
        {
            return sources.Aggregate(new SourceAggregator(ignoreAssemblyAttributeLists), (p, s) => p.AddSource(s)).GetResult();
        }

        internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives)
        {
            return input.Select(x => x.Preprocess(directives));
        }

        private enum State
        {
            OutsideIfDirective,
            SkippingIfDirective,
            KeepingIfDirective
        }

        internal static bool ParseDirective(this ReadOnlySpan<char> line, out (bool not, string symbol) result)
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

            string symbol = new string(line.Slice(index + 1).Trim());
            result = (not, symbol);

            return !string.IsNullOrEmpty(symbol);
        }

        internal static bool IsEndIfDirective(this ReadOnlySpan<char> line)
        {
            return line.TrimStart().StartsWith("#endif");
        }

        internal static bool IsElseDirective(this ReadOnlySpan<char> line)
        {
            return line.TrimStart().StartsWith("#else");
        }

        internal static string Preprocess(this IEnumerable<string> input, params string[] directives)
        {
            StringBuilder sb = new StringBuilder();
            State state = State.OutsideIfDirective;
            foreach (string line in input)
            {
                switch (state)
                {
                    case State.OutsideIfDirective:
                        {
                            ReadOnlySpan<char> span = line.AsSpan().TrimStart();
                            if (span.ParseDirective(out (bool not, string symbol) result))
                            {
                                (bool not, string symbol) = result;
                                var codeShouldBeIncluded = directives.Any(directive => symbol == directive) 
                                    ? !not : not;
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
            using (StringReader reader = new StringReader(input))
            {
                while (reader.Peek() >= 0)
                {
                    yield return reader.ReadLine();
                }
            }
        }
    }
}
