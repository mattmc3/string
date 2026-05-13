using GetOpt;

public static class SubCommand {
    private static readonly Getopt Parser = new("hqs:e:l:", ["help", "quiet", "start=", "end=", "length="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string sub [-h] [(-s | --start) START] [(-e | --end) END] [(-l | --length) LENGTH] [-q] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Extract substrings from STRING.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -s, --start INT      Start position (1-based; negative counts from end)");
        output.WriteLine("  -e, --end INT        End position, inclusive (1-based; negative counts from end)");
        output.WriteLine("  -l, --length INT     Length of substring (overrides --end)");
        output.WriteLine("  -q, --quiet          Suppress output; exit 0 if any result non-empty, 1 if all empty");
        output.WriteLine("  -h, --help           Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        int? start = null, end = null, length = null;
        bool quiet = false;

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-s":
                case "--start":
                    if (!long.TryParse(opt.Arg!, out long sv)) {
                        error.WriteLine($"error: sub: Invalid start value '{opt.Arg}'");
                        return 1;
                    }
                    start = sv > int.MaxValue ? int.MaxValue : sv < int.MinValue ? int.MinValue : (int)sv;
                    break;
                case "-e":
                case "--end":
                    if (!long.TryParse(opt.Arg!, out long ev)) {
                        error.WriteLine($"error: sub: Invalid end value '{opt.Arg}'");
                        return 1;
                    }
                    end = ev > int.MaxValue ? int.MaxValue : ev < int.MinValue ? int.MinValue : (int)ev;
                    break;
                case "-l":
                case "--length":
                    length = int.Parse(opt.Arg!);
                    break;
                case "-q":
                case "--quiet":
                    quiet = true;
                    break;
            }
        }

        if (start == 0) {
            error.WriteLine("error: sub: Invalid start value '0'");
            return 1;
        }
        if (end == 0) {
            error.WriteLine("error: sub: Invalid end value '0'");
            return 1;
        }
        if (length.HasValue && length.Value < 0) {
            error.WriteLine($"error: sub: Invalid length value '{length}'");
            return 1;
        }
        if (end.HasValue && length.HasValue) {
            error.WriteLine("error: sub: invalid option combination, --end and --length are mutually exclusive");
            return 1;
        }

        IEnumerable<string> strings = inputs.Count > 0 ? inputs : CommandUtils.ReadLines(stdin);
        bool any = false;

        foreach (var s in strings) {
            var result = Substring(s, start, end, length);
            if (result.Length > 0) {
                any = true;
            }
            if (!quiet) {
                output.WriteLine(result);
            }
        }

        return any ? 0 : 1;
    }

    private static string Substring(string s, int? start, int? end, int? length) {
        int len = s.Length;

        int startIdx = start.HasValue ? ToIndex(start.Value, len) : 0;
        startIdx = Math.Clamp(startIdx, 0, len);

        if (length.HasValue) {
            int take = Math.Max(0, Math.Min(length.Value, len - startIdx));
            return s.Substring(startIdx, take);
        }

        int endIdx = end.HasValue
            ? (end.Value >= 1 ? end.Value : Math.Clamp(len + end.Value, 0, len))
            : len;
        endIdx = Math.Clamp(endIdx, 0, len);

        if (endIdx <= startIdx) {
            return "";
        }
        return s.Substring(startIdx, endIdx - startIdx);
    }

    private static int ToIndex(long pos, int len) {
        long idx = pos >= 1 ? pos - 1 : len + pos;
        return (int)Math.Clamp(idx, 0, len);
    }
}
