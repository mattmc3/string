using GetOpt;

public static class UpperCommand {
    private static readonly Getopt Parser = new("hq", ["help", "quiet"]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string upper [-h] [-q] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Convert STRING to uppercase.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -q, --quiet    Suppress output; exit 0 if any string changed, 1 if none");
        output.WriteLine("  -h, --help     Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        var (opts, inputs) = Parser.Parse(args);
        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }
        return TransformCommand.Run(opts, inputs, stdin, s => s.ToUpperInvariant(), output);
    }
}

public static class LowerCommand {
    private static readonly Getopt Parser = new("hq", ["help", "quiet"]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string lower [-h] [-q] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Convert STRING to lowercase.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -q, --quiet    Suppress output; exit 0 if any string changed, 1 if none");
        output.WriteLine("  -h, --help     Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        var (opts, inputs) = Parser.Parse(args);
        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }
        return TransformCommand.Run(opts, inputs, stdin, s => s.ToLowerInvariant(), output);
    }
}

internal static class TransformCommand {
    internal static int Run(IReadOnlyList<OptArg> opts, IReadOnlyList<string> inputs, TextReader stdin, Func<string, string> transform, TextWriter output) {
        bool quiet = opts.Any(o => o.Opt is "-q" or "--quiet");
        IEnumerable<string> strings = CommandUtils.Strings(inputs, stdin);
        bool changes = false;
        foreach (var s in strings) {
            var result = transform(s);
            if (!quiet) {
                output.WriteLine(result);
            }
            if (result != s) {
                changes = true;
            }
        }
        return changes ? 0 : 1;
    }
}
