namespace String.Tests;

public class SplitTests : TestBase {
    [Fact]
    public void Split_basic() {
        var (exit, stdout, _) = Run("split", ".", "example.com");
        Assert.Equal(0, exit);
        Assert.Equal(["example", "com"], Lines(stdout));
    }

    [Fact]
    public void Split_no_match_returns_1() {
        var (exit, stdout, _) = Run("split", ".", "example");
        Assert.Equal(1, exit);
        Assert.Equal(["example"], Lines(stdout));
    }

    [Fact]
    public void Split_multiple_strings() {
        var (exit, stdout, _) = Run("split", ",", "a,b", "c,d");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "b", "c", "d"], Lines(stdout));
    }

    [Fact]
    public void Split_max_limits_splits() {
        var (exit, stdout, _) = Run("split", "-m", "1", "/", "/usr/local/bin");
        Assert.Equal(0, exit);
        Assert.Equal(["", "usr/local/bin"], Lines(stdout));
    }

    [Fact]
    public void Split_right_splits_from_right() {
        var (exit, stdout, _) = Run("split", "-r", "-m", "1", "/", "/usr/local/bin/fish");
        Assert.Equal(0, exit);
        Assert.Equal(["/usr/local/bin", "fish"], Lines(stdout));
    }

    [Fact]
    public void Split_no_empty_filters_empty_parts() {
        var (exit, stdout, _) = Run("split", "-n", ",", "a,,b");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "b"], Lines(stdout));
    }

    [Fact]
    public void Split_empty_sep_chars() {
        var (exit, stdout, _) = Run("split", "", "abc");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "b", "c"], Lines(stdout));
    }

    [Fact]
    public void Split_fields_selects_fields() {
        var (exit, stdout, _) = Run("split", "-f", "1,3", ",", "a,b,c");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "c"], Lines(stdout));
    }

    [Fact]
    public void Split_fields_range() {
        var (exit, stdout, _) = Run("split", "-f", "2-4", ",", "a,b,c,d,e");
        Assert.Equal(0, exit);
        Assert.Equal(["b", "c", "d"], Lines(stdout));
    }

    [Fact]
    public void Split_fields_missing_returns_1() {
        var (exit, _, _) = Run("split", "-f", "5", ",", "a,b,c");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Split_fields_allow_empty_skips_missing() {
        var (exit, stdout, _) = Run("split", "-f", "1,5", "-a", ",", "a,b,c");
        Assert.Equal(0, exit);
        Assert.Equal(["a"], Lines(stdout));
    }

    [Fact]
    public void Split_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("split", "-q", ".", "a.b");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Split_missing_sep_returns_error() {
        var (exit, _, stderr) = Run("split");
        Assert.Equal(1, exit);
        Assert.Contains("separator", stderr);
    }

    [Fact]
    public void Split_help_shows_usage() {
        var (exit, stdout, _) = Run("split", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_split() {
        var (exit, stdout, _) = RunWithStdin("a,b\nc,d\n", "split", ",");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "b", "c", "d"], Lines(stdout));
    }

    [Fact]
    public void Split0_nul_separated() {
        var (exit, stdout, _) = RunWithStdin("a\0b\0c\0", "split0");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "b", "c"], Lines(stdout));
    }

    [Fact]
    public void Split0_no_trailing_empty_from_nul() {
        var (exit, stdout, _) = RunWithStdin("a\0b\0", "split0");
        Assert.Equal(0, exit);
        Assert.Equal(["a", "b"], Lines(stdout));
    }

    [Fact]
    public void Split0_help_shows_usage() {
        var (exit, stdout, _) = Run("split0", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }
}
