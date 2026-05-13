using GetOpt;

public static class ShortenCommand {
    private static readonly Getopt Parser = new("hqNlc:m:", ["help", "quiet", "no-newline", "left", "char=", "max="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string shorten [-h] [-l] [-N] [-q] [(-c | --char) CHARS] [(-m | --max) INTEGER] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Shorten strings to a maximum width, appending an ellipsis if truncated.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -l, --left          Shorten from the left (ellipsis at start)");
        output.WriteLine("  -N, --no-newline    Omit newline after last output");
        output.WriteLine("  -c, --char CHARS    Ellipsis string (default: …)");
        output.WriteLine("  -m, --max INT       Maximum width (default: no limit)");
        output.WriteLine("  -q, --quiet         Suppress output; exit 0 if any shortened, 1 if none");
        output.WriteLine("  -h, --help          Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool quiet = false, noNewline = false, left = false;
        string ellipsis = "…";
        int max = int.MaxValue;

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-q":
                case "--quiet":
                    quiet = true;
                    break;
                case "-N":
                case "--no-newline":
                    noNewline = true;
                    break;
                case "-l":
                case "--left":
                    left = true;
                    break;
                case "-c":
                case "--char":
                    ellipsis = opt.Arg!;
                    break;
                case "-m":
                case "--max":
                    max = int.Parse(opt.Arg!);
                    break;
            }
        }

        IReadOnlyList<string> strings = inputs.Count > 0
            ? inputs
            : CommandUtils.ReadLines(stdin).ToList();

        bool changes = false;
        for (int i = 0; i < strings.Count; i++) {
            string s = strings[i];
            string result = Shorten(s, max, ellipsis, left);
            if (result != s) {
                changes = true;
            }
            if (!quiet) {
                bool isLast = i == strings.Count - 1;
                if (noNewline && isLast) {
                    output.Write(result);
                }
                else {
                    output.WriteLine(result);
                }
            }
        }

        return changes ? 0 : 1;
    }

    private static string Shorten(string s, int max, string ellipsis, bool left) {
        if (s.Length <= max) {
            return s;
        }
        int keep = Math.Max(0, max - ellipsis.Length);
        if (left) {
            return ellipsis + s[^keep..];
        }
        return s[..keep] + ellipsis;
    }
}
