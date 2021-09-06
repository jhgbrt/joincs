using Span = System.ReadOnlySpan<char>;

namespace JoinCSharp;
internal static class Preprocessor
{
    internal static IEnumerable<string> Preprocess(this IEnumerable<IEnumerable<string>> input, params string[] directives)
        => input.Select(x => string.Join(Environment.NewLine, x.Preprocess(_ => {}, directives)));

    private record struct State(Func<State, string, int, State> Next, string[] Directives, bool Done, int NonBlankLinesYielded = 0)
    {
        internal State Push()
        {
            _stack.Push(this);
            return this with { NonBlankLinesYielded = NonBlankLinesYielded + 1 };
        }
        internal State Reset() => _stack.Pop() with { Done = false, NonBlankLinesYielded = NonBlankLinesYielded };
        internal State Peek() => _stack.Peek();
        internal State Yield(string line)
        {
            _lines.Add(line);
            return this with { NonBlankLinesYielded = NonBlankLinesYielded + (string.IsNullOrWhiteSpace(line) ? 0 : 1) };
        }
        internal IEnumerable<string> GetLines()
        {
            foreach (var line in _lines) yield return line;
            _lines.Clear();
        }
        internal State AddDirective(string directive) => this with { Directives = Directives.Concat(new[] { directive }).ToArray() };
        internal State RemoveDirective(string directive) => this with { Directives = Directives.Except(new[] { directive }).ToArray() };
        private readonly Stack<State> _stack = new();
        private readonly List<string> _lines = new();

        public override string ToString()
            =>
            $"Method = {Next.Method.Name}, " +
            $"Stack = {string.Join(",", _stack.Select(f => f.Next.Method.Name))}, " +
            $"Lines = {string.Join("\\r\\n", _lines)}, " +
            $"NonBlankLines = {NonBlankLinesYielded}";

        internal Directive? GetDirective(string line) => line.AsSpan().Trim("") switch
        {
            Span { Length : 0 } => default,
            Span s when !s.StartsWith("#") => default,
            Span s when s.StartsWith("#if ") => If.From(s[3..]),
            Span s when s.StartsWith("#elif ") => ElIf.From(s[5..]),
            Span s when s.StartsWith("#else") && s.Length == 5 => new Else(),
            Span s when s.StartsWith("#endif") => new EndIf(),
            Span s when s.StartsWith("#error") => new Error(s.Length >= 7 ? s[7..].ToString() : string.Empty),
            Span s when s.StartsWith("#warning") => default,
            Span s when s.StartsWith("#line") => default,
            Span s when s.StartsWith("#region") => default,
            Span s when s.StartsWith("#endregion") => default,
            Span s when s.StartsWith("#define ") => new Define(s[8..].ToString()),
            Span s when s.StartsWith("#undef ") => new Undefine(s[7..].ToString()),
            Span s when s.StartsWith("#pragma ") => new Pragma(s[8..].ToString()),
            _ => throw new PreprocessorException($"CS1024 - Invalid preprocessor directive: {line}")
        };
    }

    internal static IEnumerable<string> Preprocess(this IEnumerable<string> input, Action<string> log, params string[] directives)
    {
        var state = new State(OutsideIfDirective, directives, false);
        foreach ((string line, int i) in input.Select((s,i) => (s,i)))
        {
            log?.Invoke(state.ToString());
            log?.Invoke(line);
            state = state.Next(state, line, i);
            log?.Invoke($"-> {state}");
            log?.Invoke("");
            foreach (var l in state.GetLines())
                yield return l;
        }
    }

    private static State OutsideIfDirective(State state, string line, int lineNumber) => state.GetDirective(line) switch
    {
        If { IsValid: false } => throw new PreprocessorException(),
        If ifd when ifd.CodeShouldBeIncluded(state.Directives) => state.Push() with { Next = KeepingCode },
        If ifd => state.Push() with { Next = SkippingCode },
        Error e => throw new PreprocessorException(e.Message),
        Define or Undefine when state.NonBlankLinesYielded > 0 => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
        Define d => state.AddDirective(d.Symbol),
        Undefine d => state.RemoveDirective(d.Symbol),
        _ when string.IsNullOrWhiteSpace(line) => state.Yield(line),
        _ => state.Yield(line)
    };

    private static State KeepingCode(State state, string line, int lineNumber) => state.GetDirective(line) switch
    {
        If { IsValid: false } => throw new PreprocessorException("CS1517: invalid preprocessor expression"),
        If ifd when ifd.CodeShouldBeIncluded(state.Directives) => state.Push(),
        If ifd => state.Push() with { Next = SkippingCode },
        EndIf => state.Reset(),
        Else => state with { Next = SkippingCode,  },
        ElIf => state with { Next = SkippingCode, Done = true },
        Define or Undefine => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
        _ => state.Yield(line)
    };

    private static State SkippingCode(State state, string line, int lineNumber) => state switch
    {
        { Done: true } => state.GetDirective(line) switch
        {
            If { IsValid: false } => throw new PreprocessorException(),
            If => state.Push(),
            EndIf => state.Reset(),
            Define or Undefine => throw new PreprocessorException("CS1032: Cannot define/undefine preprocessor symbols after first token in file"),
            _ => state
        },
        { Done: false } => state.GetDirective(line) switch
        {
            If { IsValid: false } => throw new PreprocessorException(),
            If => state.Push(),
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

    interface Directive { };

    record struct If(bool Not, string Symbol) : Directive
    {
        public static If From(Span span)
        {
            var (not, symbol) = Parse(span);
            return new(not, symbol);
        }
        public bool IsValid => !Not || (Not && !string.IsNullOrEmpty(Symbol));
        public bool CodeShouldBeIncluded(string[] directives) => directives.Contains(Symbol) ? !Not : Not;
    }

    record struct ElIf(bool Not, string Symbol) : Directive
    {
        public static ElIf From(Span span)
        {
            var (not, symbol) = Parse(span);
            return new(not, symbol);
        }

        public bool IsValid => !Not || (Not && !string.IsNullOrEmpty(Symbol));
        public bool CodeShouldBeIncluded(string[] directives) => directives.Contains(Symbol) ? !Not : Not;
    }

    record struct EndIf : Directive;
    record struct Else : Directive;
    record struct Error(string Message) : Directive;
    record struct Define(string Symbol) : Directive;
    record struct Undefine(string Symbol) : Directive;
    record struct Pragma(string Message) : Directive;

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
