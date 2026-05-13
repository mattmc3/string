using GetOpt;
using System.Text.RegularExpressions;

public static class LengthCommand {
    private static readonly Getopt Parser = new("hqV", ["help", "quiet", "visible"]);
    private static readonly Regex AnsiEscape = new(@"\x1b(?:\[[0-9;]*[a-zA-Z]|[^[])", RegexOptions.Compiled);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string length [-h] [-q] [-V] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Print the length of each STRING.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -V, --visible    Count visible width (strip ANSI escape sequences)");
        output.WriteLine("  -q, --quiet      Suppress output; exit 0 if any non-empty, 1 if all empty");
        output.WriteLine("  -h, --help       Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool quiet = false, visible = false;

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
                case "-V":
                case "--visible":
                    visible = true;
                    break;
            }
        }

        IEnumerable<string> strings = inputs.Count > 0 ? inputs : CommandUtils.ReadLines(stdin);
        bool any = false;
        foreach (var s in strings) {
            var measured = visible ? AnsiEscape.Replace(s, "") : s;
            int len = measured.Length;
            if (len > 0) {
                any = true;
            }
            if (!quiet) {
                output.WriteLine(len);
            }
        }

        return any ? 0 : 1;
    }
}
