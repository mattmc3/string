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
                    if (!int.TryParse(opt.Arg!, out count)) {
                        error.WriteLine($"error: repeat: Invalid count value '{opt.Arg}'");
                        return 1;
                    }
                    break;
                case "-m":
                case "--max":
                    if (!int.TryParse(opt.Arg!, out max) || max < 0) {
                        error.WriteLine($"error: repeat: Invalid max value '{opt.Arg}'");
                        return 1;
                    }
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

        if (count < 0) {
            error.WriteLine($"error: repeat: Invalid count value '{count}'");
            return 1;
        }

        // Positional count: if -n not given and first input parses as integer, use it as count
        if (count == 0 && inputs.Count > 0) {
            if (int.TryParse(inputs[0], out int positionalCount)) {
                if (positionalCount < 0) {
                    error.WriteLine($"error: repeat: Invalid count value '{inputs[0]}'");
                    return 1;
                }
                count = positionalCount;
                inputs = inputs.Skip(1).ToList();
            }
            else if (max < 0) {
                error.WriteLine($"error: repeat: Invalid count value '{inputs[0]}'");
                return 1;
            }
        }

        // max-only mode: no -n given, but -m given
        bool maxOnly = count == 0 && max >= 0;
        if (count == 0 && !maxOnly) {
            return 1;
        }

        if (inputs.Count > 0 && stdin != TextReader.Null) {
            var peek = stdin.Peek();
            if (peek != -1) {
                error.WriteLine("error: repeat: too many arguments");
                return 1;
            }
        }

        IReadOnlyList<string> strings = CommandUtils.StringsList(inputs, stdin);
        if (strings.Count == 0) {
            return 1;
        }

        bool changes = false;
        for (int i = 0; i < strings.Count; i++) {
            int effectiveCount = maxOnly
                ? (strings[i].Length > 0 ? (max / strings[i].Length) + 1 : 0)
                : count;
            var repeated = string.Concat(Enumerable.Repeat(strings[i], effectiveCount));
            if (max >= 0 && repeated.Length > max) {
                repeated = repeated[..max];
            }
            if (repeated.Length > 0) {
                changes = true;
            }
            if (!quiet && (repeated.Length > 0 || strings.Count > 1)) {
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
