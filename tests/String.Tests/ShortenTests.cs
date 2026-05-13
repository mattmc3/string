namespace String.Tests;

public class ShortenTests : TestBase {
    [Fact]
    public void Shorten_truncates_right_by_default() {
        var (exit, stdout, _) = Run("shorten", "-m", "5", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["hell…"], Lines(stdout));
    }

    [Fact]
    public void Shorten_truncates_left_with_flag() {
        var (exit, stdout, _) = Run("shorten", "-m", "5", "-l", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["…orld"], Lines(stdout));
    }

    [Fact]
    public void Shorten_custom_ellipsis() {
        var (exit, stdout, _) = Run("shorten", "-m", "7", "-c", "...", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["hell..."], Lines(stdout));
    }

    [Fact]
    public void Shorten_no_change_returns_0() {
        var (exit, stdout, _) = Run("shorten", "-m", "20", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Shorten_exact_length_unchanged() {
        var (exit, _, _) = Run("shorten", "-m", "5", "hello");
        Assert.Equal(0, exit);
    }

    [Fact]
    public void Shorten_multiple_strings() {
        var (exit, stdout, _) = Run("shorten", "-m", "4", "hello", "hi", "world");
        Assert.Equal(0, exit);
        Assert.Equal(["hel…", "hi", "wor…"], Lines(stdout));
    }

    [Fact]
    public void Shorten_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("shorten", "-q", "-m", "3", "hello");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Shorten_no_newline_on_last() {
        var (exit, stdout, _) = Run("shorten", "-N", "-m", "4", "hello");
        Assert.Equal(0, exit);
        Assert.Equal("hel…", stdout);
    }

    [Fact]
    public void Shorten_max_shorter_than_ellipsis_truncates_content() {
        var (exit, stdout, _) = Run("shorten", "-m", "1", "-c", "...", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["h"], Lines(stdout));
    }

    [Fact]
    public void Shorten_help_shows_usage() {
        var (exit, stdout, _) = Run("shorten", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_shorten() {
        var (exit, stdout, _) = RunWithStdin("hello world\nhi\n", "shorten", "-m", "6");
        Assert.Equal(0, exit);
        Assert.Equal(["hello…", "hi"], Lines(stdout));
    }
}
