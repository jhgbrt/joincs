namespace JoinCSharp;

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

    internal static IEnumerable<string> ReadLines(this string input)
    {
        using StringReader reader = new(input);
        while (reader.Peek() >= 0)
        {
            yield return reader.ReadLine()!;
        }
    }
}
