namespace String.Tests;

public class CollectTests : TestBase {
    [Fact]
    public void Collect_joins_args_with_newlines() {
        var (exit, stdout, _) = Run("collect", "a", "b", "c");
        Assert.Equal(0, exit);
        Assert.Equal("a\nb\nc" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Collect_stdin_trims_trailing_newline() {
        var (exit, stdout, _) = RunWithStdin("a\nb\nc\n", "collect");
        Assert.Equal(0, exit);
        Assert.Equal("a\nb\nc" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Collect_no_trim_preserves_trailing_newlines() {
        var (exit, stdout, _) = RunWithStdin("a\nb\nc\n", "collect", "-N");
        Assert.Equal(0, exit);
        Assert.Equal("a\nb\nc\n", stdout);
    }

    [Fact]
    public void Collect_empty_returns_1() {
        var (exit, stdout, _) = RunWithStdin("", "collect");
        Assert.Equal(1, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Collect_empty_allow_empty_returns_0() {
        var (exit, _, _) = RunWithStdin("", "collect", "-a");
        Assert.Equal(0, exit);
    }

    [Fact]
    public void Collect_help_shows_usage() {
        var (exit, stdout, _) = Run("collect", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }
}
