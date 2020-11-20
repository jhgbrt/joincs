using System;
using System.Collections.Generic;
using System.Linq;
using Span = System.ReadOnlySpan<char>;

namespace JoinCSharp
{
    internal static class Preprocessor
    {
        internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives)
            => input.Select(x => string.Join(Environment.NewLine, x.Preprocess(directives)));

        record State(Func<State, string, State> Next, string[] Directives, bool Done)
        {
            internal State Reset() => this with { Next = OutsideIfDirective, Done = false };
            internal State Yield(string line)
            {
                _lines.Add(line);
                return this;
            }
            public IEnumerable<string> GetLines()
            {
                foreach (var line in _lines) yield return line;
                _lines.Clear();
            }
            private readonly List<string> _lines = new();
        }

        internal static IEnumerable<string> Preprocess(this IEnumerable<string> input, params string[] directives)
        {
            var state = new State(OutsideIfDirective, directives, false);
            foreach (string line in input)
            {
                state = state.Next(state, line);
                foreach (var l in state.GetLines())
                    yield return l;
            }
        }

        static State OutsideIfDirective(State state, string line) => GetDirective(line) switch
        {
            IfDirective { IsValid: false } => state.Yield(line),
            IfDirective ifd when ifd.CodeShouldBeIncluded(state.Directives) => state with { Next = KeepingCode },
            IfDirective => state with { Next = SkippingCode },
            _ => state.Yield(line)
        };
        static State KeepingCode(State state, string line) => GetDirective(line) switch
        {
            EndIfDirective => state.Reset(),
            ElseDirective => state with { Next = SkippingCode, Done = true },
            ElseIfDirective => state with { Next = SkippingCode, Done = true },
            _ => state.Yield(line)
        };
        static State SkippingCode(State state, string line) => state switch
        {
            { Done: true } => GetDirective(line) switch
            {
                EndIfDirective => state.Reset(),
                _ => state
            },
            { Done: false } => GetDirective(line) switch
            {
                EndIfDirective => state.Reset(),
                ElseIfDirective ifd when ifd.CodeShouldBeIncluded(state.Directives) => state with { Next = KeepingCode },
                ElseIfDirective => state with { Next = SkippingCode },
                ElseDirective => state with { Next = KeepingCode },
                _ => state
            }
        };
        static object? GetDirective(string line) => line.AsSpan().TrimStart() switch
        {
            Span { Length: 0 } => default,
            Span s when s[0] != '#' => default,
            Span s when s.StartsWith("#if ") => IfDirective.From(s[3..]),
            Span s when s.StartsWith("#elif ") => ElseIfDirective.From(s[5..]),
            Span s when s.StartsWith("#else") => ElseDirective.Instance,
            Span s when s.StartsWith("#endif") => EndIfDirective.Instance,
            _ => default
        };

        record IfDirective(bool Not, string Symbol)
        {
            public static IfDirective From(Span span)
            {
                var (not, symbol) = Parse(span);
                return new IfDirective(not, symbol);
            }
            public bool IsValid => !Not || Not && !string.IsNullOrEmpty(Symbol);
            public bool CodeShouldBeIncluded(string[] directives) => directives.Any(directive => Symbol == directive) ? !Not : Not;
        }
        record ElseIfDirective(bool Not, string Symbol)
        {
            public static ElseIfDirective From(Span span)
            {
                var (not, symbol) = Parse(span);
                return new ElseIfDirective(not, symbol);
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
        static (bool not, string symbol) Parse(Span span)
        {
            var index = span.IndexOf('!');
            var not = index >= 0;
            if (!not)
            {
                index = 0;
            }
            string symbol = new(span[(index + 1)..].Trim());
            return (not, symbol);
        }
    }
}
