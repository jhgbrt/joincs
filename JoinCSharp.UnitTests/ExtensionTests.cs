using Xunit;

namespace JoinCSharp.UnitTests;

public class ExtensionTests
{

    [Fact]
    public void IsBelow_FileInFolder_True()
    {
        var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
        var root = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").DirectoryInfo;
        Assert.True(fileInfo.SitsBelow(root));
    }
    [Fact]
    public void IsBelow_FileInFolderBelow_True()
    {
        var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
        var root = PathBuilder.FromRoot().WithSubFolders("Users").DirectoryInfo;
        Assert.True(fileInfo.SitsBelow(root));
    }
    [Fact]
    public void IsBelow_FileInRootFolderBelow_True()
    {
        var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
        var root = PathBuilder.FromRoot().DirectoryInfo;
        Assert.True(fileInfo.SitsBelow(root));
    }
    [Fact]
    public void IsBelow_FileOtherFolderBelow_False()
    {
        var fileInfo = PathBuilder.FromRoot().WithSubFolders("Users", "Joe").WithFileName("tmp.txt").FileInfo;
        var root = PathBuilder.FromRoot().WithSubFolders("Users", "Jane").DirectoryInfo;
        Assert.False(fileInfo.SitsBelow(root));
    }

    [Fact]
    public void Except_FiltersFileInSubfolders()
    {
        var input = new[]
        {
            PathBuilder.FromRoot().WithSubFolders("A", "AA").WithFileName("AAA.txt").FileInfo,
            PathBuilder.FromRoot().WithSubFolders("A", "AB").WithFileName("AAB.txt").FileInfo,
            PathBuilder.FromRoot().WithSubFolders("A", "AC").WithFileName("AAC.txt").FileInfo,
            PathBuilder.FromRoot().WithSubFolders("A", "AD").WithFileName("AAD.txt").FileInfo,
            PathBuilder.FromRoot().WithSubFolders("A", "AD").WithFileName("AAE.txt").FileInfo,
            PathBuilder.FromRoot().WithSubFolders("A", "AE").WithFileName("AAF.txt").FileInfo
        };

        var subdirs = new[]
        {
            PathBuilder.FromRoot().WithSubFolders("A", "AB").DirectoryInfo,
            PathBuilder.FromRoot().WithSubFolders("A", "AD").DirectoryInfo
        };

        var result = input.Except(subdirs).Select(f => f.FullName).OrderBy(a => a).ToArray();

        var expected = new[]
        {
            PathBuilder.FromRoot().WithSubFolders("A", "AA").WithFileName("AAA.txt").FileInfo.FullName,
            PathBuilder.FromRoot().WithSubFolders("A", "AC").WithFileName("AAC.txt").FileInfo.FullName,
            PathBuilder.FromRoot().WithSubFolders("A", "AE").WithFileName("AAF.txt").FileInfo.FullName
        };

        Assert.Equal(expected, result);
    }
}