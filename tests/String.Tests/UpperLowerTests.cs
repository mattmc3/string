namespace String.Tests;

public class UpperLowerTests : TestBase {
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
    public void Lower_help_shows_usage() {
        var (exit, stdout, _) = Run("lower", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
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
    public void Upper_help_shows_usage() {
        var (exit, stdout, _) = Run("upper", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_lower() {
        var (exit, stdout, _) = RunWithStdin("Foo\nBAR\nbaz\n", "lower");
        Assert.Equal(0, exit);
        Assert.Equal(["foo", "bar", "baz"], Lines(stdout));
    }

    [Fact]
    public void Stdin_upper() {
        var (exit, stdout, _) = RunWithStdin("Foo\nbar\nBAZ\n", "upper");
        Assert.Equal(0, exit);
        Assert.Equal(["FOO", "BAR", "BAZ"], Lines(stdout));
    }

    [Fact]
    public void Stdin_args_take_priority_over_stdin() {
        var (exit, stdout, _) = RunWithStdin("IGNORED\n", "lower", "Foo");
        Assert.Equal(0, exit);
        Assert.Equal(["foo"], Lines(stdout));
    }
}
