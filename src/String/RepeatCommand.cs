using GetOpt;

public static class RepeatCommand {
    private static readonly Getopt Parser = new("hn:m:Nq", ["help", "count=", "max=", "no-newline", "quiet"]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string repeat [-h] -n COUNT [-m MAX] [-N] [-q] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Repeat STRING COUNT times.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -n, --count COUNT   Number of times to repeat (required)");
        output.WriteLine("  -m, --max MAX       Maximum length of result");
        output.WriteLine("  -N, --no-newline    Omit newline after last output");
        output.WriteLine("  -q, --quiet         Suppress output; exit 0 if any output, 1 if none");
        output.WriteLine("  -h, --help          Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        int count = 0;
        int max = -1;
        bool noNewline = false;
        bool quiet = false;

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-n":
                case "--count":
                    count = int.Parse(opt.Arg!);
                    break;
                case "-m":
                case "--max":
                    max = int.Parse(opt.Arg!);
                    break;
                case "-N":
                case "--no-newline":
                    noNewline = true;
                    break;
                case "-q":
                case "--quiet":
                    quiet = true;
                    break;
            }
        }

        if (count <= 0) {
            return 1;
        }

        IReadOnlyList<string> strings = inputs.Count > 0 ? inputs : CommandUtils.ReadLines(stdin).ToList();
        if (strings.Count == 0) {
            return 1;
        }

        bool changes = false;
        for (int i = 0; i < strings.Count; i++) {
            var repeated = string.Concat(Enumerable.Repeat(strings[i], count));
            if (max >= 0 && repeated.Length > max) {
                repeated = repeated[..max];
            }
            if (repeated.Length > 0) {
                changes = true;
            }
            if (!quiet) {
                bool isLast = i == strings.Count - 1;
                if (noNewline && isLast) {
                    output.Write(repeated);
                }
                else {
                    output.WriteLine(repeated);
                }
            }
        }

        return changes ? 0 : 1;
    }
}
