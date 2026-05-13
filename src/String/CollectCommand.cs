using GetOpt;

public static class CollectCommand {
    private static readonly Getopt Parser = new("haN", ["help", "allow-empty", "no-trim-newlines"]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string collect [-h] [-a] [-N] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Collect all strings into a single output.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -a, --allow-empty        Exit 0 even if result is empty");
        output.WriteLine("  -N, --no-trim-newlines   Preserve trailing newlines");
        output.WriteLine("  -h, --help               Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool allowEmpty = false, noTrim = false;

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-a":
                case "--allow-empty":
                    allowEmpty = true;
                    break;
                case "-N":
                case "--no-trim-newlines":
                    noTrim = true;
                    break;
            }
        }

        string collected = inputs.Count > 0
            ? string.Join('\n', inputs)
            : stdin.ReadToEnd();

        if (!noTrim) {
            collected = collected.TrimEnd('\r', '\n');
        }

        if (collected.Length == 0) {
            return allowEmpty ? 0 : 1;
        }

        if (noTrim) {
            output.Write(collected);
        }
        else {
            output.WriteLine(collected);
        }

        return 0;
    }
}
