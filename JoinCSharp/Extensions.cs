using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Span = System.ReadOnlySpan<char>;

namespace JoinCSharp
{
    internal static class Extensions
    {
        public static IEnumerable<FileInfo> Except(this IEnumerable<FileInfo> input, params DirectoryInfo[] folders) 
            => input.Where(file => !folders.Any(file.SitsBelow));

        public static DirectoryInfo SubFolder(this DirectoryInfo root, string sub) 
            => new(Path.Combine(root.FullName, sub));

        public static bool SitsBelow(this FileInfo file, DirectoryInfo folder) 
            => file.Directory?.Parents().Any(dir => dir.FullName.Equals(folder.FullName)) ?? false;

        public static IEnumerable<DirectoryInfo> Parents(this DirectoryInfo info)
        {
            var item = info;
            while (item != null)
            {
                yield return item;
                item = item.Parent;
            }
        }

        public static IEnumerable<IEnumerable<string>> ReadLines(this IEnumerable<FileInfo> input) 
            => input.Select(f => File.ReadLines(f.FullName));

        public static string Aggregate(this IEnumerable<string> sources, bool includeAssemblyAttributes = false)
            => sources.Aggregate(new SourceAggregator(includeAssemblyAttributes), (p, s) => p.AddSource(s)).GetResult();

        internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives) 
            => input.Select(x => string.Join(Environment.NewLine, x.Preprocess(directives)));

        record State(ProcessLine NextProcessor, string? LineToYield);

        delegate State ProcessLine(string line, string[] directives);
        
        internal static IEnumerable<string> Preprocess(this IEnumerable<string> input, params string[] directives)
        {
            ProcessLine processLine = OutsideIfDirective;

            foreach (string line in input)
            {
                var next = processLine(line, directives);

                if (next.LineToYield != default)
                    yield return next.LineToYield;

                processLine = next.NextProcessor;
            }

            static State OutsideIfDirective(string line, string[] directives) => GetDirective(line) switch
            {
                IfDirective { IsValid: false }  => new(OutsideIfDirective, line),
                IfDirective ifd => ifd.CodeShouldBeIncluded(directives) switch
                {
                    true => new (KeepingIfDirective, null),
                    false => new (SkippingIfDirective, null)
                },
                _ => new(OutsideIfDirective, line)
            };

            static State KeepingIfDirective(string line, string[] directives) => GetDirective(line) switch
            {
                EndIfDirective => new(OutsideIfDirective, null),
                ElseDirective => new(SkippingIfDirective, null),
                _ => new(KeepingIfDirective, line)
            };

            static State SkippingIfDirective(string line, string[] directives) => GetDirective(line) switch
            {
                EndIfDirective => new(OutsideIfDirective, null),
                ElseDirective => new(KeepingIfDirective, null),
                _ => new(SkippingIfDirective, null)
            };

            static object? GetDirective(string line) => line.AsSpan().TrimStart() switch
            {
                Span s when s.Length == 0 || s[0] != '#' => default,
                Span s when s.StartsWith("#if ") => IfDirective.From(s),
                Span s when s.StartsWith("#else") => ElseDirective.Instance,
                Span s when s.StartsWith("#endif") => EndIfDirective.Instance,
                _ => default
            };
        }

        record IfDirective(bool Not, string Symbol) 
        {
            public static IfDirective From(ReadOnlySpan<char> span)
            {
                var index = span.IndexOf('!');
                var not = index >= 0;
                if (!not)
                {
                    index = 2;
                }

                string symbol = new(span[(index + 1)..].Trim());
                return new IfDirective(not, symbol);
            }
            public bool IsValid => !Not || Not && !string.IsNullOrEmpty(Symbol);
            public bool CodeShouldBeIncluded(string[] directives) => directives.Any(directive => Symbol == directive) ? !Not : Not;
        }

        record EndIfDirective 
        {
            public static EndIfDirective Instance = new EndIfDirective();
        }

        record ElseDirective 
        {
            public static ElseDirective Instance = new ElseDirective();
        }

        internal static IEnumerable<string> ReadLines(this string input)
        {
            using StringReader reader = new(input);
            while (reader.Peek() >= 0)
            {
                yield return reader.ReadLine()!;
            }
        }
    }
}
