namespace String.Tests;

public class JoinTests : TestBase {
    [Fact]
    public void Join_basic() {
        var (exit, stdout, _) = Run("join", ",", "a", "b", "c");
        Assert.Equal(0, exit);
        Assert.Equal("a,b,c" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Join_single_string_returns_1() {
        var (exit, stdout, _) = Run("join", ",", "a");
        Assert.Equal(1, exit);
        Assert.Equal("a" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Join_no_strings_returns_1() {
        var (exit, _, _) = RunWithStdin("", "join", ",");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Join_no_empty_filters_empty_strings() {
        var (exit, stdout, _) = Run("join", "-n", "+", "a", "b", "", "c");
        Assert.Equal(0, exit);
        Assert.Equal("a+b+c" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Join_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("join", "-q", ",", "a", "b");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Join_empty_sep() {
        var (exit, stdout, _) = Run("join", "", "a", "b", "c");
        Assert.Equal(0, exit);
        Assert.Equal("abc" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Join_missing_sep_returns_error() {
        var (exit, _, stderr) = Run("join");
        Assert.Equal(1, exit);
        Assert.Contains("separator", stderr);
    }

    [Fact]
    public void Join_help_shows_usage() {
        var (exit, stdout, _) = Run("join", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_join() {
        var (exit, stdout, _) = RunWithStdin("a\nb\nc\n", "join", "...");
        Assert.Equal(0, exit);
        Assert.Equal("a...b...c" + Environment.NewLine, stdout);
    }

    [Fact]
    public void Join0_nul_separated_with_trailing_nul() {
        var (exit, stdout, _) = Run("join0", "a", "b", "c");
        Assert.Equal(0, exit);
        Assert.Equal("a\0b\0c\0", stdout);
    }

    [Fact]
    public void Join0_roundtrips_with_split0() {
        var joined = new StringWriter();
        var err = new StringWriter();
        StringApp.Run(["join0", "a", "b", "c"], TextReader.Null, joined, err);

        var split = new StringWriter();
        StringApp.Run(["split0"], new StringReader(joined.ToString()), split, err);
        Assert.Equal(["a", "b", "c"], Lines(split.ToString()));
    }

    [Fact]
    public void Join0_help_shows_usage() {
        var (exit, stdout, _) = Run("join0", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }
}
