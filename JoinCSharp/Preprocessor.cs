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

        private record State(Func<State, string, State> Next, string[] Directives, bool Done)
        {
            internal State Push()
            {
                _stack.Push(this);
                return this;
            }
            internal State Reset() => _stack.Pop() with { Done = false };
            internal State Peek() => _stack.Peek();
            internal State Yield(string line)
            {
                _lines.Add(line);
                return this;
            }
            internal IEnumerable<string> GetLines()
            {
                foreach (var line in _lines) yield return line;
                _lines.Clear();
            }
            internal State AddDirective(string directive) => this with { Directives = Directives.Concat(new[] { directive }).ToArray() };
            internal State RemoveDirective(string directive) => this with { Directives = Directives.Except(new[] { directive }).ToArray() };
            private Stack<State> _stack = new();
            private readonly List<string> _lines = new();

            public override string ToString()
                =>
                $"Method = {Next.Method.Name}, " +
                $"Stack = {string.Join(",", _stack.Select(f => f.Next.Method.Name))}, " +
                $"Lines = {string.Join("\\r\\n", _lines)}";
        }

        internal static IEnumerable<string> Preprocess(this IEnumerable<string> input, Action<string> log, params string[] directives)
        {
            var state = new State(BeginningOfFile, directives, false);
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

        private static State BeginningOfFile(State state, string line) => GetDirective(line) switch
        {
            If { IsValid: false } => throw new PreprocessorException(),
            If ifd when ifd.CodeShouldBeIncluded(state.Directives) => (state with { Next = OutsideIfDirective }).Push() with { Next = KeepingCode },
            If ifd => (state with { Next = OutsideIfDirective }).Push() with { Next = SkippingCode },
            Error e => throw new PreprocessorException(e.Message),
            Define d => state.AddDirective(d.Symbol),
            Undefine d => state.RemoveDirective(d.Symbol),
            _ when string.IsNullOrWhiteSpace(line) => state.Yield(line),
            _ => state.Yield(line) with { Next = OutsideIfDirective }
        };
        private static State OutsideIfDirective(State state, string line) => GetDirective(line) switch
        {
            If { IsValid: false } => throw new PreprocessorException(),
            If ifd when ifd.CodeShouldBeIncluded(state.Directives) => state.Push() with { Next = KeepingCode },
            If ifd => state.Push() with { Next = SkippingCode },
            Error e => throw new PreprocessorException(e.Message),
            Define or Undefine => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
            _ => state.Yield(line)
        };

        private static State KeepingCode(State state, string line) => GetDirective(line) switch
        {
            If { IsValid: false } => throw new PreprocessorException(),
            If ifd when ifd.CodeShouldBeIncluded(state.Directives) => state.Push(),
            If ifd => state.Push() with { Next = SkippingCode },
            EndIf => state.Reset(),
            Else => state with { Next = SkippingCode,  },
            ElIf => state with { Next = SkippingCode, Done = true },
            Define or Undefine => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
            _ => state.Yield(line)
        };

        private static State SkippingCode(State state, string line) => state switch
        {
            { Done: true } => GetDirective(line) switch
            {
                If { IsValid: false } => throw new PreprocessorException(),
                If ifd => state.Push(),
                EndIf => state.Reset(),
                Define or Undefine => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
                _ => state
            },
            { Done: false } => GetDirective(line) switch
            {
                If { IsValid: false } => throw new PreprocessorException(),
                If ifd => state.Push(),
                EndIf => state.Reset(),
                Else ifd when state.Peek().Next == SkippingCode => state.Peek(),
                ElIf ifd when ifd.CodeShouldBeIncluded(state.Directives) => state with { Next = KeepingCode },
                ElIf => state with { Next = SkippingCode },
                Else => state with { Next = KeepingCode },
                Error e => throw new PreprocessorException(e.Message),
                Define or Undefine => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
                _ => state
            }
        };

        private static object? GetDirective(string line) => line.AsSpan().Trim("") switch
        {
            Span { Length: 0 } => default,
            Span s when s[0] != '#' => default,
            Span s when s.StartsWith("#if ") => If.From(s[3..]),
            Span s when s.StartsWith("#elif ") => ElIf.From(s[5..]),
            Span s when s.StartsWith("#else") && s.Length == 5 => Else.Instance,
            Span s when s.StartsWith("#endif") => EndIf.Instance,
            Span s when s.StartsWith("#error") => Error.From(s.Length >= 7 ? s[7..] : string.Empty),
            Span s when s.StartsWith("#warning") => default,
            Span s when s.StartsWith("#line") => default,
            Span s when s.StartsWith("#region") => default,
            Span s when s.StartsWith("#endregion") => default,
            Span s when s.StartsWith("#define ") => Define.From(s[8..]),
            Span s when s.StartsWith("#undef ") => Undefine.From(s[7..]),
            _ => throw new PreprocessorException("CS1024 - Invalid preprocessor directive: {}")
        };

        record If(bool Not, string Symbol)
        {
            public static If From(Span span)
            {
                var (not, symbol) = Parse(span);
                return new(not, symbol);
            }
            public bool IsValid => !Not || (Not && !string.IsNullOrEmpty(Symbol));
            public bool CodeShouldBeIncluded(string[] directives) => directives.Any(directive => Symbol == directive) ? !Not : Not;
        }

        record ElIf(bool Not, string Symbol)
        {
            public static ElIf From(Span span)
            {
                var (not, symbol) = Parse(span);
                return new(not, symbol);
            }

            public bool IsValid => !Not || (Not && !string.IsNullOrEmpty(Symbol));
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

        record Error(string Message)
        {
            public static Error From(Span message) => new Error(message.ToString());
        }

        record Define(string Symbol)
        {
            public static Define From(Span symbol) => new Define(symbol.ToString());
        }
        record Undefine(string Symbol)
        {
            public static Undefine From(Span symbol) => new Undefine(symbol.ToString());
        }

        private static (bool not, string symbol) Parse(Span span)
        {
            var index = span.IndexOf('!');
            var not = index >= 0;
            if (!not) index = 0;
            string symbol = new(span[(index + 1)..].Trim());
            return (not, symbol);
        }
    }

    internal class PreprocessorException: Exception
    {
        public PreprocessorException() {}
        public PreprocessorException(string? message) : base(message) {}
        public PreprocessorException(string? message, Exception? innerException) : base(message, innerException) {}
    }
}