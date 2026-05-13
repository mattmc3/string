namespace String.Tests;

public class StringAppTests : TestBase {
    [Fact]
    public void No_args_returns_error() {
        var (exit, _, stderr) = Run();
        Assert.Equal(1, exit);
        Assert.Contains("string: missing subcommand", stderr);
    }

    [Fact]
    public void Unknown_command_returns_error() {
        var (exit, _, stderr) = Run("flip", "hello");
        Assert.Equal(1, exit);
        Assert.Contains("string flip: invalid subcommand", stderr);
    }

    [Fact]
    public void Help_long_flag_shows_commands() {
        var (exit, stdout, _) = Run("--help");
        Assert.Equal(0, exit);
        Assert.Contains("Commands:", stdout);
    }

    [Fact]
    public void Help_short_flag_shows_commands() {
        var (exit, stdout, _) = Run("-h");
        Assert.Equal(0, exit);
        Assert.Contains("Commands:", stdout);
    }

    [Fact]
    public void Help_subcommand_shows_commands() {
        var (exit, stdout, _) = Run("help");
        Assert.Equal(0, exit);
        Assert.Contains("Commands:", stdout);
    }
}
