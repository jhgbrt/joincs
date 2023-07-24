namespace JoinCSharp;

static class Program
{
    /// <summary>
    /// A command line tool to merge a set of C# files into one single file.
    /// </summary>
    /// <param name="input">The folder containing the C# files you want to merge</param>
    /// <param name="output">Target file name (e.g. 'output.cs'). If not provided, the output is written to the console.</param>
    /// <param name="includeAssemblyAttributes"></param>
    /// <param name="preprocessorDirectives">A list of preprocessor directives that should be defined. Code between undefined #if/#endif directives is ignored.</param>
    /// <returns></returns>
    public static async Task<int> Main(
        DirectoryInfo input,
        FileInfo? output = null,
        bool includeAssemblyAttributes = false,
        string[]? preprocessorDirectives = null)
    {
        if (input == null)
        {
            Console.Error.WriteLine("input folder is required");
            return 1;
        }

        if (!input.Exists)
        {
            Console.Error.WriteLine("input folder does not exist");
            return 1;
        }

        preprocessorDirectives ??= Array.Empty<string>();

        var binobj = new[] { "bin", "obj" }
            .Select(s => Path.Combine(input.FullName, s))
            .ToArray();

        try
        {
            var result = input
                .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                .Where(f => !binobj.Any(d => f.DirectoryName?.StartsWith(d) ?? false))
                .ReadLines()
                .Preprocess(preprocessorDirectives)
                .Aggregate(includeAssemblyAttributes);

            if (output != null)
            {
                await File.WriteAllTextAsync(output.FullName, result);
            }
            else
            {
                await Console.Out.WriteAsync(result);
            }
        }
        catch (PreprocessorException e)
        {
            Console.WriteLine(e.Message);
        }

        return 0;
    }
}
