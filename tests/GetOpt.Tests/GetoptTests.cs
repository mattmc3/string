using GetOpt;

namespace GetOpt.Tests;

public class GetoptTests {
    [Fact]
    public void Short_flag() {
        var (opts, args) = new Getopt("lr").Parse(["-l"]);
        Assert.Equal([new OptArg("-l")], opts);
        Assert.Empty(args);
    }

    [Fact]
    public void Short_flags_combined() {
        var (opts, args) = new Getopt("lr").Parse(["-lr"]);
        Assert.Equal([new OptArg("-l"), new OptArg("-r")], opts);
        Assert.Empty(args);
    }

    [Fact]
    public void Short_option_value_separate() {
        var (opts, _) = new Getopt("c:").Parse(["-c", "xyz"]);
        Assert.Equal([new OptArg("-c", "xyz")], opts);
    }

    [Fact]
    public void Short_option_value_inline() {
        var (opts, _) = new Getopt("c:").Parse(["-cxyz"]);
        Assert.Equal([new OptArg("-c", "xyz")], opts);
    }

    [Fact]
    public void Long_flag() {
        var (opts, args) = new Getopt(longOpts: ["left", "right"]).Parse(["--left"]);
        Assert.Equal([new OptArg("--left")], opts);
        Assert.Empty(args);
    }

    [Fact]
    public void Long_option_value_eq() {
        var (opts, _) = new Getopt(longOpts: ["chars="]).Parse(["--chars=xyz"]);
        Assert.Equal([new OptArg("--chars", "xyz")], opts);
    }

    [Fact]
    public void Long_option_value_separate() {
        var (opts, _) = new Getopt(longOpts: ["chars="]).Parse(["--chars", "xyz"]);
        Assert.Equal([new OptArg("--chars", "xyz")], opts);
    }

    [Fact]
    public void Double_dash_stops_parsing() {
        var (opts, args) = new Getopt("lr").Parse(["-l", "--", "-r"]);
        Assert.Equal([new OptArg("-l")], opts);
        Assert.Equal(["-r"], args);
    }

    [Fact]
    public void Non_option_args_collected() {
        var (opts, args) = new Getopt("lr").Parse(["-l", "foo", "bar"]);
        Assert.Equal([new OptArg("-l")], opts);
        Assert.Equal(["foo", "bar"], args);
    }

    [Fact]
    public void Mixed_opts_and_args() {
        var (opts, args) = new Getopt("lr").Parse(["-l", "foo", "-r", "bar"]);
        Assert.Equal([new OptArg("-l"), new OptArg("-r")], opts);
        Assert.Equal(["foo", "bar"], args);
    }

    [Fact]
    public void Unknown_short_opt_throws() {
        Assert.Throws<ArgumentException>(() => new Getopt("lr").Parse(["-z"]));
    }

    [Fact]
    public void Unknown_long_opt_throws() {
        Assert.Throws<ArgumentException>(() => new Getopt(longOpts: ["left"]).Parse(["--nope"]));
    }

    [Fact]
    public void Missing_short_arg_throws() {
        Assert.Throws<ArgumentException>(() => new Getopt("c:").Parse(["-c"]));
    }

    [Fact]
    public void Missing_long_arg_throws() {
        Assert.Throws<ArgumentException>(() => new Getopt(longOpts: ["chars="]).Parse(["--chars"]));
    }

    [Fact]
    public void No_args() {
        var (opts, args) = new Getopt("lr").Parse([]);
        Assert.Empty(opts);
        Assert.Empty(args);
    }

    [Fact]
    public void Optional_arg_inline() {
        var (opts, _) = new Getopt("c::").Parse(["-cxyz"]);
        Assert.Equal([new OptArg("-c", "xyz")], opts);
    }

    [Fact]
    public void Optional_arg_absent() {
        var (opts, args) = new Getopt("c::l").Parse(["-c", "-l"]);
        Assert.Equal([new OptArg("-c", null), new OptArg("-l")], opts);
        Assert.Empty(args);
    }

    [Fact]
    public void Optional_arg_standalone() {
        var (opts, args) = new Getopt("c::").Parse(["-c", "foo"]);
        Assert.Equal([new OptArg("-c", null)], opts);
        Assert.Equal(["foo"], args);
    }

    [Fact]
    public void Posix_mode_stops_at_first_non_option() {
        var (opts, args) = new Getopt("+lr").Parse(["-l", "foo", "-r"]);
        Assert.Equal([new OptArg("-l")], opts);
        Assert.Equal(["foo", "-r"], args);
    }

    [Fact]
    public void Posix_mode_all_opts_before_args() {
        var (opts, args) = new Getopt("+lr").Parse(["-l", "-r", "foo", "bar"]);
        Assert.Equal([new OptArg("-l"), new OptArg("-r")], opts);
        Assert.Equal(["foo", "bar"], args);
    }

    [Fact]
    public void Gnu_mode_permutes_args() {
        var (opts, args) = new Getopt("lr").Parse(["-l", "foo", "-r", "bar"]);
        Assert.Equal([new OptArg("-l"), new OptArg("-r")], opts);
        Assert.Equal(["foo", "bar"], args);
    }
}
