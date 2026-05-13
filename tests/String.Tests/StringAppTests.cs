namespace String.Tests;

public class StringAppTests {
    private static (int exitCode, string stdout, string stderr) Run(params string[] args) {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var exit = StringApp.Run(args, stdout, stderr);
        return (exit, stdout.ToString(), stderr.ToString());
    }

    private static string[] Lines(string stdout) =>
        stdout.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');

    [Fact]
    public void Lower_converts_to_lowercase() {
        var (exit, stdout, _) = Run("lower", "Foo", "BAR", "baz");
        Assert.Equal(0, exit);
        Assert.Equal(["foo", "bar", "baz"], Lines(stdout));
    }

    [Fact]
    public void Lower_returns_1_when_nothing_changed() {
        var (exit, _, _) = Run("lower", "foo", "bar");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Lower_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("lower", "-q", "FOO");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void Upper_converts_to_uppercase() {
        var (exit, stdout, _) = Run("upper", "Foo", "bar", "BAZ");
        Assert.Equal(0, exit);
        Assert.Equal(["FOO", "BAR", "BAZ"], Lines(stdout));
    }

    [Fact]
    public void Upper_returns_1_when_nothing_changed() {
        var (exit, _, _) = Run("upper", "FOO", "BAR");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Upper_quiet_suppresses_output() {
        var (exit, stdout, _) = Run("upper", "-q", "foo");
        Assert.Equal(0, exit);
        Assert.Empty(stdout);
    }

    [Fact]
    public void No_args_returns_error() {
        var (exit, _, stderr) = Run();
        Assert.Equal(1, exit);
        Assert.Contains("Usage:", stderr);
    }

    [Fact]
    public void Unknown_command_returns_error() {
        var (exit, _, stderr) = Run("flip", "hello");
        Assert.Equal(1, exit);
        Assert.Contains("unknown command", stderr);
    }

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
}
