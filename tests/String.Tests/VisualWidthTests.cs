namespace String.Tests;

public class VisualWidthTests {
    // VisualWidth.Of -- single-line visual width

    [Fact]
    public void Of_plain_string() {
        Assert.Equal(5, VisualWidth.Of("hello"));
    }

    [Fact]
    public void Of_empty_string() {
        Assert.Equal(0, VisualWidth.Of(""));
    }

    [Fact]
    public void Of_strips_ansi_escape() {
        Assert.Equal(3, VisualWidth.Of("\x1b[31mabc\x1b[0m"));
    }

    [Fact]
    public void Of_backspace_decrements() {
        Assert.Equal(2, VisualWidth.Of("abc\b"));
    }

    [Fact]
    public void Of_backspace_floors_at_zero() {
        Assert.Equal(0, VisualWidth.Of("\b\b\b"));
    }

    [Fact]
    public void Of_bell_is_zero_width() {
        Assert.Equal(3, VisualWidth.Of("\afoo"));
    }

    [Fact]
    public void Of_control_chars_are_zero_width() {
        Assert.Equal(3, VisualWidth.Of("\x07\x0c\x0efoo"));
    }

    // VisualWidth.OfLines -- splits on \n, handles \r

    [Fact]
    public void OfLines_single_line() {
        Assert.Equal([3], VisualWidth.OfLines("foo").ToArray());
    }

    [Fact]
    public void OfLines_splits_on_newline() {
        Assert.Equal([3, 2], VisualWidth.OfLines("foo\nab").ToArray());
    }

    [Fact]
    public void OfLines_carriage_return_resets_position() {
        // "abcdef\r" resets to 0, then "foobaraaa" = 9 chars
        Assert.Equal([9], VisualWidth.OfLines("abcdef\rfooba\x1b[31mraaa").ToArray());
    }

    [Fact]
    public void OfLines_ansi_ignored() {
        Assert.Equal([2], VisualWidth.OfLines("a\x1b[34mb").ToArray());
    }

    [Fact]
    public void OfLines_backspace() {
        Assert.Equal([0], VisualWidth.OfLines("\b").ToArray());
    }

    // VisualWidth.TakeLeft -- prefix with target visual width

    [Fact]
    public void TakeLeft_normal_string() {
        Assert.Equal("fo", VisualWidth.TakeLeft("foobar", 2));
    }

    [Fact]
    public void TakeLeft_zero_width() {
        Assert.Equal("", VisualWidth.TakeLeft("foobar", 0));
    }

    [Fact]
    public void TakeLeft_backspace_in_string() {
        Assert.Equal("\ba", VisualWidth.TakeLeft("\babc", 1));
    }

    [Fact]
    public void TakeLeft_control_chars_included_free() {
        Assert.Equal("\afoo", VisualWidth.TakeLeft("\afoobar", 3));
    }

    [Fact]
    public void TakeLeft_ansi_sequence_not_counted() {
        Assert.Equal("\x1b[31mhel", VisualWidth.TakeLeft("\x1b[31mhello\x1b[0m", 3));
    }

    [Fact]
    public void TakeLeft_ansi_sequence_zero_width_included() {
        Assert.Equal("\x1b[31mhello", VisualWidth.TakeLeft("\x1b[31mhello\x1b[0m", 5));
    }

    // VisualWidth.TakeRight -- suffix with target visual width

    [Fact]
    public void TakeRight_normal_string() {
        Assert.Equal("ar", VisualWidth.TakeRight("foobar", 2));
    }

    [Fact]
    public void TakeRight_zero_width() {
        Assert.Equal("", VisualWidth.TakeRight("foobar", 0));
    }

    [Fact]
    public void TakeRight_full_string() {
        Assert.Equal("foo", VisualWidth.TakeRight("foo", 3));
    }

    [Fact]
    public void TakeRight_ansi_sequence_not_counted() {
        Assert.Equal("llo\x1b[0m", VisualWidth.TakeRight("\x1b[31mhello\x1b[0m", 3));
    }

    [Fact]
    public void TakeRight_ansi_only_prefix() {
        Assert.Equal("\x1b[31mhello\x1b[0m", VisualWidth.TakeRight("\x1b[31mhello\x1b[0m", 5));
    }
}
