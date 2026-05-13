using GetOpt;

public static class TrimCommand {
    private static readonly Getopt Parser = new("+hlrqc:", ["help", "left", "right", "quiet", "chars="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string trim [-h] [-l] [-r] [-q] [-c CHARS] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Remove leading and trailing whitespace from STRING.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -l, --left          Trim leading whitespace only");
        output.WriteLine("  -r, --right         Trim trailing whitespace only");
        output.WriteLine("  -c, --chars CHARS   Trim CHARS instead of whitespace");
        output.WriteLine("  -q, --quiet         Suppress output; exit 0 if any string trimmed, 1 if none");
        output.WriteLine("  -h, --help          Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool left = false, right = false, quiet = false;
        string? chars = null;

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) { WriteHelp(output); return 0; }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-l": case "--left": left = true; break;
                case "-r": case "--right": right = true; break;
                case "-q": case "--quiet": quiet = true; break;
                case "-c": case "--chars": chars = opt.Arg; break;
            }
        }

        if (!left && !right) { left = true; right = true; }

        IEnumerable<string> strings = inputs.Count > 0 ? inputs : CommandUtils.ReadLines(stdin);
        bool changes = false;
        foreach (var s in strings) {
            var result = chars is null
                ? Trim(s, left, right)
                : Trim(s, left, right, chars.ToCharArray());
            if (!quiet) output.WriteLine(result);
            if (result.Length < s.Length) changes = true;
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
}
