using System;
using System.Collections.Generic;
using System.Linq;
using Span = System.ReadOnlySpan<char>;

namespace JoinCSharp
{
    internal static class Preprocessor
    {
        internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives)
            => input.Select(x => string.Join(Environment.NewLine, x.Preprocess(_ => {}, directives)));

        record State(Func<State, string, State> Next, string[] Directives, bool Done)
        {
            private Stack<State> _stack = new();
            internal State Push()
            {
                _stack.Push(this);
                return this;
            }
            internal State Reset() => _stack.Pop() with { Done = false };
            internal Func<State, string, State> Peek() => _stack.Peek().Next;
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

            public override string ToString()
            {
                return $"Method = {Next.Method.Name}, Stack = {string.Join(",", _stack.Select(f => f.Next.Method.Name))}, Lines = {string.Join("\\r\\n", _lines)}";
            }
        }

        internal static IEnumerable<string> Preprocess(this IEnumerable<string> input, Action<string> log, params string[] directives)
        {
            var state = new State(OutsideIfDirective, directives, false);
            foreach (string line in input)
            {
                log?.Invoke(state.ToString());
                log?.Invoke(line);
                state = state.Next(state, line);
                log?.Invoke($"-> {state}");
                log?.Invoke("");
                foreach (var l in state.GetLines())
                    yield return l;
            }
        }

        static State OutsideIfDirective(State state, string line) => GetDirective(line) switch
        {
            If { IsValid: false } => throw new InvalidPreprocessorDirectiveException(),
            If ifd when ifd.CodeShouldBeIncluded(state.Directives) => state.Push() with { Next = KeepingCode },
            If ifd => state.Push() with { Next = SkippingCode },
            _ => state.Yield(line)
        };
        static State KeepingCode(State state, string line) => GetDirective(line) switch
        {
            If { IsValid: false } => throw new InvalidPreprocessorDirectiveException(),
            If ifd when ifd.CodeShouldBeIncluded(state.Directives) => state.Push() with { Next = KeepingCode },
            If ifd => state.Push() with { Next = SkippingCode },
            EndIf => state.Reset(),
            Else => state with { Next = SkippingCode,  },
            ElIf => state with { Next = SkippingCode, Done = true },
            _ => state.Yield(line)
        };
        static State SkippingCode(State state, string line) => state switch
        {
            { Done: true } => GetDirective(line) switch
            {
                If { IsValid: false } => throw new InvalidPreprocessorDirectiveException(),
                If ifd => state.Push() with { Next = SkippingCode },
                EndIf => state.Reset(),
                _ => state
            },
            { Done: false } => GetDirective(line) switch
            {
                If { IsValid: false } => throw new InvalidPreprocessorDirectiveException(),
                If ifd => state.Push() with { Next = SkippingCode },
                EndIf => state.Reset(),
                Else ifd when state.Peek() == SkippingCode => state,
                ElIf ifd when ifd.CodeShouldBeIncluded(state.Directives) => state with { Next = KeepingCode },
                ElIf => state with { Next = SkippingCode },
                Else => state with { Next = KeepingCode },
                _ => state
            }
        };
        static object? GetDirective(string line) => line.AsSpan().TrimStart("") switch
        {
            Span { Length: 0 } => default,
            Span s when s[0] != '#' => default,
            Span s when s.StartsWith("#if ") => If.From(s[3..]),
            Span s when s.StartsWith("#elif ") => ElIf.From(s[5..]),
            Span s when s.StartsWith("#else") => Else.Instance,
            Span s when s.StartsWith("#endif") => EndIf.Instance,
            _ => throw new InvalidPreprocessorDirectiveException("CS1024 - Invalid preprocessor directive: {}")
        };

        record If(bool Not, string Symbol)
        {
            public static If From(Span span)
            {
                var (not, symbol) = Parse(span);
                return new If(not, symbol);
            }
            public bool IsValid => !Not || Not && !string.IsNullOrEmpty(Symbol);
            public bool CodeShouldBeIncluded(string[] directives) => directives.Any(directive => Symbol == directive) ? !Not : Not;
        }
        record ElIf(bool Not, string Symbol)
        {
            public static ElIf From(Span span)
            {
                var (not, symbol) = Parse(span);
                return new ElIf(not, symbol);
            }

            public bool IsValid => !Not || Not && !string.IsNullOrEmpty(Symbol);
            public bool CodeShouldBeIncluded(string[] directives) => directives.Any(directive => Symbol == directive) ? !Not : Not;
        }
        record EndIf
        {
            public static EndIf Instance = new();
        }
        record Else
        {
            public static Else Instance = new();
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

    internal class InvalidPreprocessorDirectiveException: Exception
    {
        public InvalidPreprocessorDirectiveException() : base()
        {
        }

        public InvalidPreprocessorDirectiveException(string? message) : base(message)
        {
        }

        public InvalidPreprocessorDirectiveException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
