using Xunit;

namespace JoinCSharp.UnitTests;

public class ProgramTests
{
    [Fact]
    public async Task Main_WhenInputDirectoryIsNull_Returns1()
    {
        Assert.Equal(1, await Program.Main(null, null, false, null));
    }
    [Fact]
    public async Task Main_WhenInputDirectoryDoesNotExist_Returns1()
    {
        Assert.Equal(1, await Program.Main(new DirectoryInfo("NonExisting"), null, false, null));
    }
    [Fact]
    public async Task Main_WhenInputDirectoryExists_Returns0()
    {
        Assert.Equal(0, await Program.Main(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory), null, false, null));
    }
}
