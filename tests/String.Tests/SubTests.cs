namespace String.Tests;

public class SubTests : TestBase {
    [Fact]
    public void Sub_no_args_returns_full_string() {
        var (exit, stdout, _) = Run("sub", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Sub_start_from_position() {
        var (exit, stdout, _) = Run("sub", "-s", "2", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["ello"], Lines(stdout));
    }

    [Fact]
    public void Sub_start_from_end() {
        var (exit, stdout, _) = Run("sub", "-s", "-1", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["o"], Lines(stdout));
    }

    [Fact]
    public void Sub_end_position() {
        var (exit, stdout, _) = Run("sub", "-e", "3", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hel"], Lines(stdout));
    }

    [Fact]
    public void Sub_end_from_end() {
        var (exit, stdout, _) = Run("sub", "-e", "-2", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hel"], Lines(stdout));
    }

    [Fact]
    public void Sub_length_limits_output() {
        var (exit, stdout, _) = Run("sub", "-l", "3", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hel"], Lines(stdout));
    }

    [Fact]
    public void Sub_start_and_length() {
        var (exit, stdout, _) = Run("sub", "-s", "2", "-l", "3", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["ell"], Lines(stdout));
    }

    [Fact]
    public void Sub_end_and_length_mutually_exclusive() {
        var (exit, _, stderr) = Run("sub", "-e", "5", "-l", "2", "hello");
        Assert.Equal(1, exit);
        Assert.Contains("mutually exclusive", stderr);
    }

    [Fact]
    public void Sub_start_beyond_length_returns_empty() {
        var (exit, stdout, _) = Run("sub", "-s", "99", "hello");
        Assert.Equal(1, exit);
        Assert.Equal([""], Lines(stdout));
    }

    [Fact]
    public void Sub_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("sub", "-q", "-l", "3", "hello");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Sub_multiple_strings() {
        var (exit, stdout, _) = Run("sub", "-l", "2", "hello", "world");
        Assert.Equal(0, exit);
        Assert.Equal(["he", "wo"], Lines(stdout));
    }

    [Fact]
    public void Sub_help_shows_usage() {
        var (exit, stdout, _) = Run("sub", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_sub() {
        var (exit, stdout, _) = RunWithStdin("hello\nworld\n", "sub", "-l", "3");
        Assert.Equal(0, exit);
        Assert.Equal(["hel", "wor"], Lines(stdout));
    }
}
