using System.Collections.ObjectModel;

namespace JoinCSharp.UnitTests;

abstract class PathBuilder
{
    public static PathBuilder FromRoot() => new DirectoryPathBuilder(Path.DirectorySeparatorChar.ToString());
    public static PathBuilder Directory(params string[] folderPath) => new DirectoryPathBuilder(folderPath);
    public static PathBuilder File(params string[] fullPath) => 
        fullPath.Length >= 1
        ? new FilePathBuilder(fullPath.Take(fullPath.Length - 1), fullPath.Last())
        : new FilePathBuilder(Enumerable.Empty<string>(), string.Empty);

    public abstract PathBuilder WithSubFolders(params string[] folders);
    public abstract PathBuilder WithFileName(string fileName);

    public override string ToString() => FullName;
    public string FullName => FileSystemInfo.FullName;
    public abstract FileInfo FileInfo { get; }
    public abstract DirectoryInfo DirectoryInfo { get; }
    public abstract FileSystemInfo FileSystemInfo { get; }

    class DirectoryPathBuilder : PathBuilder
    {
        private readonly ReadOnlyCollection<string> _folders;

        internal DirectoryPathBuilder(IEnumerable<string> folders) => _folders = folders.ToList().AsReadOnly();
        internal DirectoryPathBuilder(params string[] folders) => _folders = folders.ToList().AsReadOnly();

        public override PathBuilder WithSubFolders(params string[] folders) => new DirectoryPathBuilder(_folders.Concat(folders));
        public override PathBuilder WithFileName(string fileName) => new FilePathBuilder(_folders, fileName);

        public override FileInfo FileInfo => throw new InvalidOperationException("Not a file");
        public override DirectoryInfo DirectoryInfo => new(Path.Combine(_folders.ToArray()));
        public override FileSystemInfo FileSystemInfo => DirectoryInfo;
    }

    class FilePathBuilder : PathBuilder
    {
        private readonly ReadOnlyCollection<string> _folders;
        private readonly string _file;

        internal FilePathBuilder(IEnumerable<string> folders, string file)
        {
            _folders = folders.ToList().AsReadOnly();
            _file = file;
        }

        public override PathBuilder WithSubFolders(params string[] folders) => new FilePathBuilder(_folders.Concat(folders), _file);
        public override PathBuilder WithFileName(string fileName) => new FilePathBuilder(_folders, fileName);

        public override FileInfo FileInfo => !string.IsNullOrEmpty(_file) ? new FileInfo(Path.Combine(_folders.Append(_file).ToArray())) : throw new InvalidOperationException("Path does not have filename");
        public override DirectoryInfo DirectoryInfo => FileInfo.Directory;
        public override FileSystemInfo FileSystemInfo => FileInfo;
    }
}