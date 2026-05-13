namespace String.Tests;

public class StringAppTests {
    private static (int exitCode, string stdout, string stderr) Run(params string[] args) {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var exit = StringApp.Run(args, stdout, stderr);
        return (exit, stdout.ToString(), stderr.ToString());
    }

    [Fact]
    public void Lower_converts_to_lowercase() {
        var (exit, stdout, _) = Run("lower", "Foo", "BAR", "baz");
        Assert.Equal(0, exit);
        Assert.Equal("foo\nbar\nbaz\n", stdout.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void Upper_converts_to_uppercase() {
        var (exit, stdout, _) = Run("upper", "Foo", "bar", "BAZ");
        Assert.Equal(0, exit);
        Assert.Equal("FOO\nBAR\nBAZ\n", stdout.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void No_args_returns_error() {
        var (exit, _, stderr) = Run();
        Assert.Equal(1, exit);
        Assert.Contains("Usage:", stderr);
    }

    [Fact]
    public void Unknown_command_throws() {
        Assert.Throws<Exception>(() => Run("flip", "hello"));
    }
}
