using GetOpt;

public static class JoinCommand {
    public static void WriteHelp(TextWriter output) => JoinCore.WriteHelp("join", output);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        var (opts, rest) = JoinCore.Parser.Parse(args);
        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        if (rest.Count == 0) {
            error.WriteLine("error: join requires a separator");
            return 1;
        }

        return JoinCore.Run(rest[0], false, opts, rest.Skip(1).ToList(), stdin, output, error);
    }
}

public static class Join0Command {
    public static void WriteHelp(TextWriter output) => JoinCore.WriteHelp("join0", output);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        var (opts, inputs) = JoinCore.Parser.Parse(args);
        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }
        return JoinCore.Run("\0", true, opts, inputs, stdin, output, error);
    }
}

internal static class JoinCore {
    internal static readonly Getopt Parser = new("+hqn", ["help", "quiet", "no-empty"]);

    internal static void WriteHelp(string name, TextWriter output) {
        bool nul0 = name == "join0";
        if (nul0) {
            output.WriteLine("Usage: string join0 [-h] [-q] [-n] [--] [STRING ...]");
        }
        else {
            output.WriteLine("Usage: string join [-h] [-q] [-n] [--] SEP [STRING ...]");
        }
        output.WriteLine();
        output.WriteLine(nul0
            ? "  Join strings with NUL (\\0) separator and a trailing NUL."
            : "  Join strings with SEP separator.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -n, --no-empty    Exclude empty strings");
        output.WriteLine("  -q, --quiet       Suppress output; exit 0 if any strings joined, 1 if none");
        output.WriteLine("  -h, --help        Show this help message");
    }

    internal static int Run(string sep, bool appendNul, IReadOnlyList<OptArg> opts, IReadOnlyList<string> inputs, TextReader stdin, TextWriter output, TextWriter error) {
        bool quiet = false, noEmpty = false;

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-q":
                case "--quiet":
                    quiet = true;
                    break;
                case "-n":
                case "--no-empty":
                    noEmpty = true;
                    break;
            }
        }

        IEnumerable<string> source = CommandUtils.Strings(inputs, stdin);
        var strings = (noEmpty ? source.Where(s => s.Length > 0) : source).ToList();

        if (strings.Count == 0) {
            return 1;
        }

        if (!quiet) {
            output.Write(string.Join(sep, strings));
            if (appendNul) {
                output.Write('\0');
            }
            else {
                output.WriteLine();
            }
        }

        return strings.Count >= 2 ? 0 : 1;
    }
}
