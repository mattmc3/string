namespace String.Tests;

public class RepeatTests : TestBase {
    [Fact]
    public void Repeat_basic() {
        var (exit, stdout, _) = Run("repeat", "-n", "3", "ab");
        Assert.Equal(0, exit);
        Assert.Equal(["ababab"], Lines(stdout));
    }

    [Fact]
    public void Repeat_multiple_strings() {
        var (exit, stdout, _) = Run("repeat", "-n", "2", "ab", "cd");
        Assert.Equal(0, exit);
        Assert.Equal(["abab", "cdcd"], Lines(stdout));
    }

    [Fact]
    public void Repeat_max_truncates() {
        var (exit, stdout, _) = Run("repeat", "-n", "3", "-m", "4", "ab");
        Assert.Equal(0, exit);
        Assert.Equal(["abab"], Lines(stdout));
    }

    [Fact]
    public void Repeat_no_newline_suppresses_trailing_newline() {
        var (exit, stdout, _) = Run("repeat", "-n", "2", "-N", "ab");
        Assert.Equal(0, exit);
        Assert.Equal("abab", stdout);
    }

    [Fact]
    public void Repeat_no_newline_only_suppresses_last() {
        var (exit, stdout, _) = Run("repeat", "-n", "2", "-N", "ab", "cd");
        Assert.Equal(0, exit);
        Assert.Equal($"abab{Environment.NewLine}cdcd", stdout);
    }

    [Fact]
    public void Repeat_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("repeat", "-n", "3", "-q", "ab");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Repeat_returns_1_when_count_zero() {
        var (exit, _, _) = Run("repeat", "-n", "0", "ab");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Repeat_returns_1_with_no_strings() {
        var (exit, _, _) = Run("repeat", "-n", "3");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Repeat_help_shows_usage() {
        var (exit, stdout, _) = Run("repeat", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_repeat() {
        var (exit, stdout, _) = RunWithStdin("ab\ncd\n", "repeat", "-n", "2");
        Assert.Equal(0, exit);
        Assert.Equal(["abab", "cdcd"], Lines(stdout));
    }
}
