namespace String.Tests;

public class ReplaceTests : TestBase {
    [Fact]
    public void Replace_literal_first_only() {
        var (exit, stdout, _) = Run("replace", "o", "0", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["f0obar"], Lines(stdout));
    }

    [Fact]
    public void Replace_literal_all() {
        var (exit, stdout, _) = Run("replace", "-a", "o", "0", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["f00bar"], Lines(stdout));
    }

    [Fact]
    public void Replace_no_match_returns_1() {
        var (exit, stdout, _) = Run("replace", "x", "y", "foobar");
        Assert.Equal(1, exit);
        Assert.Equal(["foobar"], Lines(stdout));
    }

    [Fact]
    public void Replace_ignore_case() {
        var (exit, stdout, _) = Run("replace", "-i", "FOO", "baz", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["bazbar"], Lines(stdout));
    }

    [Fact]
    public void Replace_regex_basic() {
        var (exit, stdout, _) = Run("replace", "-r", "o+", "0", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["f0bar"], Lines(stdout));
    }

    [Fact]
    public void Replace_regex_all() {
        var (exit, stdout, _) = Run("replace", "-r", "-a", "o", "0", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["f00bar"], Lines(stdout));
    }

    [Fact]
    public void Replace_regex_backreference() {
        var (exit, stdout, _) = Run("replace", "-r", "(foo)(bar)", "$2$1", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["barfoo"], Lines(stdout));
    }

    [Fact]
    public void Replace_regex_ignore_case() {
        var (exit, stdout, _) = Run("replace", "-r", "-i", "FOO", "baz", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["bazbar"], Lines(stdout));
    }

    [Fact]
    public void Replace_filter_only_prints_changed() {
        var (exit, stdout, _) = Run("replace", "-f", "o", "0", "foobar", "hello", "baz");
        Assert.Equal(0, exit);
        Assert.Equal(["f0obar", "hell0"], Lines(stdout));
    }

    [Fact]
    public void Replace_max_matches_limits_replacements() {
        var (exit, stdout, _) = Run("replace", "-a", "-m", "2", "o", "0", "foooobar");
        Assert.Equal(0, exit);
        Assert.Equal(["f00oobar"], Lines(stdout));
    }

    [Fact]
    public void Replace_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("replace", "-q", "o", "0", "foobar");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Replace_multiple_strings() {
        var (exit, stdout, _) = Run("replace", "a", "x", "bar", "baz", "nope");
        Assert.Equal(0, exit);
        Assert.Equal(["bxr", "bxz", "nope"], Lines(stdout));
    }

    [Fact]
    public void Replace_missing_args_returns_error() {
        var (exit, _, stderr) = Run("replace", "only-pattern");
        Assert.Equal(1, exit);
        Assert.Contains("replacement", stderr);
    }

    [Fact]
    public void Replace_help_shows_usage() {
        var (exit, stdout, _) = Run("replace", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_replace() {
        var (exit, stdout, _) = RunWithStdin("foobar\nhello\n", "replace", "-a", "o", "0");
        Assert.Equal(0, exit);
        Assert.Equal(["f00bar", "hell0"], Lines(stdout));
    }
}
