using GetOpt;

public static class PadCommand {
    private static readonly Getopt Parser = new("hrCc:w:", ["help", "right", "center", "char=", "width="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string pad [-h] [-r] [-C] [(-c | --char) CHAR] [(-w | --width) INTEGER] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Pad strings to a fixed width.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -r, --right         Pad on the right (left-align)");
        output.WriteLine("  -C, --center        Center the string");
        output.WriteLine("  -c, --char CHAR     Character to use for padding (default: space)");
        output.WriteLine("  -w, --width INT     Target width (default: longest string)");
        output.WriteLine("  -h, --help          Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool right = false, center = false;
        char padChar = ' ';
        int? width = null;

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-r":
                case "--right":
                    right = true;
                    break;
                case "-C":
                case "--center":
                    center = true;
                    break;
                case "-c":
                case "--char":
                    if (opt.Arg!.Length != 1) {
                        error.WriteLine("error: pad character must be exactly one character");
                        return 1;
                    }
                    padChar = opt.Arg[0];
                    break;
                case "-w":
                case "--width":
                    width = int.Parse(opt.Arg!);
                    break;
            }
        }

        IReadOnlyList<string> strings = inputs.Count > 0 ? inputs : CommandUtils.ReadLines(stdin).ToList();
        if (strings.Count == 0) {
            return 1;
        }

        int targetWidth = width ?? strings.Max(s => s.Length);
        bool changes = false;

        foreach (var s in strings) {
            string result;
            if (s.Length >= targetWidth) {
                result = s;
            }
            else {
                int total = targetWidth - s.Length;
                if (center) {
                    int leftPad = total / 2;
                    int rightPad = total - leftPad;
                    result = new string(padChar, leftPad) + s + new string(padChar, rightPad);
                }
                else if (right) {
                    result = s + new string(padChar, total);
                }
                else {
                    result = new string(padChar, total) + s;
                }
                changes = true;
            }
            output.WriteLine(result);
        }

        return changes ? 0 : 1;
    }
}
