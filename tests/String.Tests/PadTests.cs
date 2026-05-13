namespace String.Tests;

public class PadTests : TestBase {
    [Fact]
    public void Pad_default_pads_left() {
        var (exit, stdout, _) = Run("pad", "-w", "5", "hi");
        Assert.Equal(0, exit);
        Assert.Equal(["   hi"], Lines(stdout));
    }

    [Fact]
    public void Pad_right_pads_right() {
        var (exit, stdout, _) = Run("pad", "-r", "-w", "5", "hi");
        Assert.Equal(0, exit);
        Assert.Equal(["hi   "], Lines(stdout));
    }

    [Fact]
    public void Pad_center_centers_string() {
        var (exit, stdout, _) = Run("pad", "-C", "-w", "7", "hi");
        Assert.Equal(0, exit);
        Assert.Equal(["   hi  "], Lines(stdout));
    }

    [Fact]
    public void Pad_custom_char() {
        var (exit, stdout, _) = Run("pad", "-w", "5", "-c", ".", "hi");
        Assert.Equal(0, exit);
        Assert.Equal(["...hi"], Lines(stdout));
    }

    [Fact]
    public void Pad_auto_width_uses_longest() {
        var (exit, stdout, _) = Run("pad", "hi", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["   hi", "hello"], Lines(stdout));
    }

    [Fact]
    public void Pad_string_at_width_unchanged() {
        var (exit, stdout, _) = Run("pad", "-w", "5", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Pad_string_longer_than_width_unchanged() {
        var (exit, stdout, _) = Run("pad", "-w", "3", "hello");
        Assert.Equal(0, exit);
        Assert.Equal(["hello"], Lines(stdout));
    }

    [Fact]
    public void Pad_invalid_char_returns_error() {
        var (exit, _, stderr) = Run("pad", "-c", "ab", "hi");
        Assert.Equal(1, exit);
        Assert.Contains("error", stderr);
    }

    [Fact]
    public void Pad_help_shows_usage() {
        var (exit, stdout, _) = Run("pad", "--help");
        Assert.Equal(0, exit);
        Assert.Contains("Usage:", stdout);
    }

    [Fact]
    public void Stdin_pad() {
        var (exit, stdout, _) = RunWithStdin("hi\nhello\n", "pad", "-w", "7");
        Assert.Equal(0, exit);
        Assert.Equal(["     hi", "  hello"], Lines(stdout));
    }
}
