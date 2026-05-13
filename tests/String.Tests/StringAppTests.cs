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
}
