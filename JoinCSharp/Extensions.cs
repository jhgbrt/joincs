using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace JoinCSharp
{
    public static class Extensions
    {
        public static string Join(
            this IEnumerable<string> sources,
            params string[] preprocessorSymbols
            )
        {
            return sources.Aggregate(new SourceAggregator(preprocessorSymbols?.ToArray()), (p, s) => p.AddSource(s)).GetResult();
        }
        internal static IEnumerable<T> WithoutTrivia<T>(this IEnumerable<T> input) where T : SyntaxNode
        {
            return input.Select(x => x.WithoutTrivia());
        }
    }
}
