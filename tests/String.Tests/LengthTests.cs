namespace String.Tests;

public class LengthTests : TestBase {
    [Fact]
    public void Length_single_string() {
        var (exit, stdout, _) = Run("length", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["5"], Lines(stdout));
    }

    [Fact]
    public void Length_multiple_strings() {
        var (exit, stdout, _) = Run("length", "foo", "hello", "ab");
        Assert.Equal(0, exit);
        Assert.Equal(["3", "5", "2"], Lines(stdout));
    }

    [Fact]
    public void Length_empty_string_returns_1() {
        var (exit, stdout, _) = Run("length", "");
        Assert.Equal(1, exit);
        Assert.Equal(["0"], Lines(stdout));
    }

    [Fact]
    public void Length_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("length", "-q", "hello");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Length_quiet_empty_returns_1() {
        var (exit, _, _) = Run("length", "-q", "");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Length_visible_strips_ansi() {
        var (exit, stdout, _) = Run("length", "-V", "\x1b[31mhello\x1b[0m");
        Assert.Equal(0, exit);
        Assert.Equal(["5"], Lines(stdout));
    }

    [Fact]
    public void Length_visible_no_ansi_same_as_normal() {
        var (exit, stdout, _) = Run("length", "-V", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["5"], Lines(stdout));
    }

    [Fact]
    public void Length_help_shows_usage() {
        var (exit, stdout, _) = Run("length", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_length() {
        var (exit, stdout, _) = RunWithStdin("foo\nhello\n", "length");
        Assert.Equal(0, exit);
        Assert.Equal(["3", "5"], Lines(stdout));
    }
}
