namespace String.Tests;

public class MatchTests : TestBase {
    [Fact]
    public void Match_glob_exact() {
        var (exit, stdout, _) = Run("match", "foo", "foo");
        Assert.Equal(0, exit);
        Assert.Equal(["foo"], Lines(stdout));
    }

    [Fact]
    public void Match_glob_star() {
        var (exit, stdout, _) = Run("match", "foo*", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["foobar"], Lines(stdout));
    }

    [Fact]
    public void Match_glob_question() {
        var (exit, stdout, _) = Run("match", "f?o", "foo");
        Assert.Equal(0, exit);
        Assert.Equal(["foo"], Lines(stdout));
    }

    [Fact]
    public void Match_glob_char_class() {
        var (exit, stdout, _) = Run("match", "[abc]oo", "boo");
        Assert.Equal(0, exit);
        Assert.Equal(["boo"], Lines(stdout));
    }

    [Fact]
    public void Match_glob_negated_class() {
        var (exit, stdout, _) = Run("match", "[!abc]oo", "zoo");
        Assert.Equal(0, exit);
        Assert.Equal(["zoo"], Lines(stdout));
    }

    [Fact]
    public void Match_glob_no_match_returns_1() {
        var (exit, _, _) = Run("match", "foo", "bar");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Match_glob_ignore_case() {
        var (exit, stdout, _) = Run("match", "-i", "FOO*", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["foobar"], Lines(stdout));
    }

    [Fact]
    public void Match_regex_basic() {
        var (exit, stdout, _) = Run("match", "-r", "fo+", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["foo"], Lines(stdout));
    }

    [Fact]
    public void Match_regex_groups_only() {
        var (exit, stdout, _) = Run("match", "-r", "-g", "f(o+)(bar)", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["oo", "bar"], Lines(stdout));
    }

    [Fact]
    public void Match_regex_groups_only_no_groups_returns_1() {
        var (exit, _, _) = Run("match", "-r", "-g", "foobar", "foobar");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Match_all_finds_multiple() {
        var (exit, stdout, _) = Run("match", "-r", "-a", "o+", "foobaroo");
        Assert.Equal(0, exit);
        Assert.Equal(["oo", "oo"], Lines(stdout));
    }

    [Fact]
    public void Match_entire_prints_full_string() {
        var (exit, stdout, _) = Run("match", "-r", "-e", "fo+", "foobar");
        Assert.Equal(0, exit);
        Assert.Equal(["foobar"], Lines(stdout));
    }

    [Fact]
    public void Match_index_prints_position() {
        var (exit, stdout, _) = Run("match", "-r", "-n", "fo+", "xfoobar");
        Assert.Equal(0, exit);
        Assert.Equal(["2 3"], Lines(stdout));
    }

    [Fact]
    public void Match_invert_prints_non_matching() {
        var (exit, stdout, _) = Run("match", "-v", "foo", "foo", "bar", "baz");
        Assert.Equal(0, exit);
        Assert.Equal(["bar", "baz"], Lines(stdout));
    }

    [Fact]
    public void Match_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("match", "-q", "foo*", "foobar");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Match_max_matches_limits_results() {
        var (exit, stdout, _) = Run("match", "-r", "-a", "-m", "2", "o", "foooo");
        Assert.Equal(0, exit);
        Assert.Equal(["o", "o"], Lines(stdout));
    }

    [Fact]
    public void Match_missing_pattern_returns_error() {
        var (exit, _, stderr) = Run("match");
        Assert.Equal(1, exit);
        Assert.Contains("pattern", stderr);
    }

    [Fact]
    public void Match_help_shows_usage() {
        var (exit, stdout, _) = Run("match", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_match_glob() {
        var (exit, stdout, _) = RunWithStdin("foobar\nbaz\nfoo\n", "match", "foo*");
        Assert.Equal(0, exit);
        Assert.Equal(["foobar", "foo"], Lines(stdout));
    }

    [Fact]
    public void Stdin_match_regex() {
        var (exit, stdout, _) = RunWithStdin("foobar\nbaz\n", "match", "-r", "fo+");
        Assert.Equal(0, exit);
        Assert.Equal(["foo"], Lines(stdout));
    }
}
