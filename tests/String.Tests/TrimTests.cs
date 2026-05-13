namespace String.Tests;

public class TrimTests : TestBase {
    [Fact]
    public void Trim_both_sides_by_default() {
        var (exit, stdout, _) = Run("trim", "  hello  ");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Trim_left_only() {
        var (exit, stdout, _) = Run("trim", "-l", "  hello  ");
        Assert.Equal(0, exit);
        Assert.Equal(["hello  "], Lines(stdout));
    }

    [Fact]
    public void Trim_right_only() {
        var (exit, stdout, _) = Run("trim", "-r", "  hello  ");
        Assert.Equal(0, exit);
        Assert.Equal(["  hello"], Lines(stdout));
    }

    [Fact]
    public void Trim_custom_chars() {
        var (exit, stdout, _) = Run("trim", "-c", "xy", "xxhelloyx");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Trim_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("trim", "-q", "  hello  ");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Trim_returns_1_when_nothing_trimmed() {
        var (exit, _, _) = Run("trim", "hello");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Trim_multiple_strings() {
        var (exit, stdout, _) = Run("trim", "  foo  ", "  bar  ");
        Assert.Equal(0, exit);
        Assert.Equal(["foo", "bar"], Lines(stdout));
    }

    [Fact]
    public void Trim_help_shows_usage() {
        var (exit, stdout, _) = Run("trim", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_trim() {
        var (exit, stdout, _) = RunWithStdin("  foo  \n  bar  \n", "trim");
        Assert.Equal(0, exit);
        Assert.Equal(["foo", "bar"], Lines(stdout));
    }

    [Fact]
    public void Stdin_trim_with_flag() {
        var (exit, stdout, _) = RunWithStdin("  foo  \n  bar  \n", "trim", "-l");
        Assert.Equal(0, exit);
        Assert.Equal(["foo  ", "bar  "], Lines(stdout));
    }
}
