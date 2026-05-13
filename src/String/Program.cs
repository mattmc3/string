using GetOpt;

return StringApp.Run(args, Console.Out, Console.Error);

public static class StringApp {
    public static int Run(string[] args, TextWriter output, TextWriter error) {
        if (args.Length < 1) {
            error.WriteLine("Usage: string <command> [options] [STRING ...]");
            error.WriteLine("Commands: upper, lower, trim");
            return 1;
        }

        var command = args[0];
        var rest = args[1..];

        return command switch {
            "upper" => RunSimple(rest, s => s.ToUpperInvariant(), output, error),
            "lower" => RunSimple(rest, s => s.ToLowerInvariant(), output, error),
            "trim" => RunTrim(rest, output, error),
            "repeat" => RunRepeat(rest, output, error),
            _ => UnknownCommand(command, error),
        };
    }

    private static readonly Getopt SimpleParser = new("q", ["quiet"]);

    private static int RunSimple(string[] args, Func<string, string> transform, TextWriter output, TextWriter error) {
        var (opts, inputs) = SimpleParser.Parse(args);
        bool quiet = opts.Any(o => o.Opt is "-q" or "--quiet");
        bool changes = false;
        foreach (var s in inputs) {
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

    private static int RunTrim(string[] args, TextWriter output, TextWriter error) {
        bool left = false, right = false, quiet = false;
        string? chars = null;

        var parser = new Getopt("+lrqc:", ["left", "right", "quiet", "chars="]);
        var (opts, inputs) = parser.Parse(args);

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-l": case "--left": left = true; break;
                case "-r": case "--right": right = true; break;
                case "-q": case "--quiet": quiet = true; break;
                case "-c": case "--chars": chars = opt.Arg; break;
            }
        }

        if (!left && !right) {
            left = true;
            right = true;
        }

        bool changes = false;
        foreach (var s in inputs) {
            var result = chars is null
                ? Trim(s, left, right)
                : Trim(s, left, right, chars.ToCharArray());
            if (!quiet) {
                output.WriteLine(result);
            }
            if (result.Length < s.Length) {
                changes = true;
            }
        }

        return changes ? 0 : 1;
    }

    private static string Trim(string s, bool left, bool right) =>
        (left, right) switch {
            (true, true) => s.Trim(),
            (true, false) => s.TrimStart(),
            (false, true) => s.TrimEnd(),
            _ => s,
        };

    private static string Trim(string s, bool left, bool right, char[] chars) =>
        (left, right) switch {
            (true, true) => s.Trim(chars),
            (true, false) => s.TrimStart(chars),
            (false, true) => s.TrimEnd(chars),
            _ => s,
        };

    private static readonly Getopt RepeatParser = new("n:m:Nq", ["count=", "max=", "no-newline", "quiet"]);

    private static int RunRepeat(string[] args, TextWriter output, TextWriter error) {
        int count = 0;
        int max = -1;
        bool noNewline = false;
        bool quiet = false;

        var (opts, inputs) = RepeatParser.Parse(args);

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-n": case "--count": count = int.Parse(opt.Arg!); break;
                case "-m": case "--max": max = int.Parse(opt.Arg!); break;
                case "-N": case "--no-newline": noNewline = true; break;
                case "-q": case "--quiet": quiet = true; break;
            }
        }

        if (count <= 0 || inputs.Count == 0) {
            return 1;
        }

        bool changes = false;
        for (int i = 0; i < inputs.Count; i++) {
            var repeated = string.Concat(Enumerable.Repeat(inputs[i], count));
            if (max >= 0 && repeated.Length > max) {
                repeated = repeated[..max];
            }
            if (repeated.Length > 0) {
                changes = true;
            }
            if (!quiet) {
                bool isLast = i == inputs.Count - 1;
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

    private static int UnknownCommand(string command, TextWriter error) {
        error.WriteLine($"error: unknown command '{command}'");
        return 1;
    }
}
