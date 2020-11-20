using System;
using Xunit;

namespace JoinCSharp.UnitTests
{
    public class PathBuilderTests
    {
        [Fact]
        public void PathBuilder_FromRoot_Exists()
        {
            Assert.True(PathBuilder.FromRoot().DirectoryInfo.Exists);
        }
        [Fact]
        public void PathBuilder_FromRoot_WithSubfolders()
        {
            string fullName = PathBuilder.FromRoot().WithSubFolders("hello").DirectoryInfo.FullName;
            Assert.EndsWith("hello", fullName);
        }
        [Fact]
        public void PathBuilder_FromRoot_WithFile()
        {
            string fullName = PathBuilder.FromRoot().WithFileName("hello.txt").FileInfo.FullName;
            Assert.EndsWith("hello.txt", fullName);
        }
        [Fact]
        public void PathBuilder_Directory_WhenAskingForFileInfo_Throws()
        {
            var builder = PathBuilder.FromRoot();
            Assert.Throws<InvalidOperationException>(() => builder.FileInfo);
        }
        [Fact]
        public void PathBuilder_Directory_WhenAskingForFileSystemInfo_ReturnsDirectoryInfo()
        {
            var builder = PathBuilder.Directory().WithSubFolders(".");

            Assert.Equal(builder.DirectoryInfo.FullName, builder.FileSystemInfo.FullName);

        }
        [Fact]
        public void PathBuilder_File_WhenAskingForFileSystemInfo_ReturnsFileInfo()
        {
            var builder = PathBuilder.File().WithSubFolders(".").WithFileName("hello.txt");

            Assert.Equal(builder.FileInfo.FullName, builder.FileSystemInfo.FullName);

        }
        [Fact]
        public void PathBuilder_File_DirectoryInfo_Returns()
        {
            var builder = PathBuilder.File().WithSubFolders(".").WithFileName("hello.txt");

            Assert.Equal(builder.FileInfo.Directory.FullName, builder.DirectoryInfo.FullName);
        }
        [Fact]
        public void PathBuilder_ToString_ReturnsFullPath()
        {
            var builder = PathBuilder.File().WithSubFolders(".").WithFileName("hello.txt");

            Assert.Equal(builder.ToString(), builder.FullName);
        }

        //[Theory]
        //[InlineData("")]
        //[InlineData("test.txt", "test.txt")]
        //[InlineData("test.txt", "subfolder", "test.txt")]
        //public void PathBuilderX(string expected, params string[] parts)
        //{
        //    var result = PathBuilder.File(parts).FileInfo.Name;
        //    Assert.Equal(@"test.txt", result);
        //}
    }
}
