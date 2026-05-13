namespace String.Tests;

public class EscapeTests : TestBase {
    [Fact]
    public void Escape_script_wraps_in_single_quotes() {
        var (exit, stdout, _) = Run("escape", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["'hello world'"], Lines(stdout));
    }

    [Fact]
    public void Escape_script_safe_string_still_quoted() {
        var (exit, stdout, _) = Run("escape", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["'hello'"], Lines(stdout));
    }

    [Fact]
    public void Escape_script_no_quoted_skips_safe_string() {
        var (exit, stdout, _) = Run("escape", "-n", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Escape_script_no_quoted_quotes_unsafe_string() {
        var (exit, stdout, _) = Run("escape", "-n", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["'hello world'"], Lines(stdout));
    }

    [Fact]
    public void Escape_script_embeds_single_quote() {
        var (exit, stdout, _) = Run("escape", "it's");
        Assert.Equal(0, exit);
        Assert.Equal(["'it'\\''s'"], Lines(stdout));
    }

    [Fact]
    public void Escape_url_encodes_special_chars() {
        var (exit, stdout, _) = Run("escape", "--style=url", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["hello%20world"], Lines(stdout));
    }

    [Fact]
    public void Escape_html_encodes_angle_brackets() {
        var (exit, stdout, _) = Run("escape", "--style=html", "<b>bold</b>");
        Assert.Equal(0, exit);
        Assert.Equal(["&lt;b&gt;bold&lt;/b&gt;"], Lines(stdout));
    }

    [Fact]
    public void Escape_regex_escapes_metacharacters() {
        var (exit, stdout, _) = Run("escape", "--style=regex", "a.b*c");
        Assert.Equal(0, exit);
        Assert.Equal([@"a\.b\*c"], Lines(stdout));
    }

    [Fact]
    public void Escape_var_encodes_non_alphanumeric() {
        var (exit, stdout, _) = Run("escape", "--style=var", "hello world");
        Assert.Equal(0, exit);
        Assert.Equal(["hello_20_world"], Lines(stdout));
    }

    [Fact]
    public void Escape_empty_returns_1() {
        var (exit, _, _) = RunWithStdin("", "escape");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Escape_help_shows_usage() {
        var (exit, stdout, _) = Run("escape", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Escape_stdin_processes_lines() {
        var (exit, stdout, _) = RunWithStdin("hello\nworld\n", "escape", "-n");
        Assert.Equal(0, exit);
        Assert.Equal(["hello", "world"], Lines(stdout));
    }
}

public class UnescapeTests : TestBase {
    [Fact]
    public void Unescape_script_removes_single_quotes() {
        var (exit, stdout, _) = Run("unescape", "'hello world'");
        Assert.Equal(0, exit);
        Assert.Equal(["hello world"], Lines(stdout));
    }

    [Fact]
    public void Unescape_script_handles_embedded_quote() {
        var (exit, stdout, _) = Run("unescape", "'it'\\''s'");
        Assert.Equal(0, exit);
        Assert.Equal(["it's"], Lines(stdout));
    }

    [Fact]
    public void Unescape_url_decodes() {
        var (exit, stdout, _) = Run("unescape", "--style=url", "hello%20world");
        Assert.Equal(0, exit);
        Assert.Equal(["hello world"], Lines(stdout));
    }

    [Fact]
    public void Unescape_html_decodes() {
        var (exit, stdout, _) = Run("unescape", "--style=html", "&lt;b&gt;bold&lt;/b&gt;");
        Assert.Equal(0, exit);
        Assert.Equal(["<b>bold</b>"], Lines(stdout));
    }

    [Fact]
    public void Unescape_regex_unescapes() {
        var (exit, stdout, _) = Run("unescape", "--style=regex", @"a\.b\*c");
        Assert.Equal(0, exit);
        Assert.Equal(["a.b*c"], Lines(stdout));
    }

    [Fact]
    public void Unescape_var_decodes() {
        var (exit, stdout, _) = Run("unescape", "--style=var", "hello_20_world");
        Assert.Equal(0, exit);
        Assert.Equal(["hello world"], Lines(stdout));
    }

    [Fact]
    public void Unescape_empty_returns_1() {
        var (exit, _, _) = RunWithStdin("", "unescape");
        Assert.Equal(1, exit);
    }

    [Fact]
    public void Unescape_help_shows_usage() {
        var (exit, stdout, _) = Run("unescape", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }
}
