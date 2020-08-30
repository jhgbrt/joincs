using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JoinCSharp.UnitTests
{
    class PathBuilder
    {
        private List<string> _folders = new();
        private string? _file = null;
        public static PathBuilder FromRoot()
        {
            return new PathBuilder().WithSubFolders(Path.DirectorySeparatorChar.ToString());
        }
        public PathBuilder WithSubFolders(params string[] folders)
        {
            _folders.AddRange(folders);
            return this;
        }
        public PathBuilder WithFileName(string fileName)
        {
            _file = fileName;
            return this;
        }
        public override string ToString()
        {
            return FullPath;
        }
        public string FullPath => Path.Combine(_folders.Append(_file).ToArray());
        public FileInfo FileInfo => !string.IsNullOrEmpty(_file) ? new FileInfo(Path.Combine(_folders.Append(_file).ToArray())) : throw new InvalidOperationException("Path does not have filename");
        public DirectoryInfo DirectoryInfo  => string.IsNullOrEmpty(_file) ? new DirectoryInfo(Path.Combine(_folders.ToArray())) : throw new InvalidOperationException("Path is a filename");
    }
}