return StringApp.Run(args, Console.In, Console.Out, Console.Error);

public static class StringApp {
    public static int Run(string[] args, TextWriter output, TextWriter error) =>
        Run(args, TextReader.Null, output, error);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        if (args.Length < 1) {
            error.WriteLine("Usage: string <command> [options] [STRING ...]");
            error.WriteLine("Commands: upper, lower, trim, repeat, match");
            error.WriteLine("Use 'string --help' for more information.");
            return 1;
        }

        var command = args[0];
        if (command is "--help" or "-h" or "help") {
            WriteHelp(output);
            return 0;
        }

        var rest = args[1..];

        return command switch {
            "upper" => UpperCommand.Run(rest, stdin, output, error),
            "lower" => LowerCommand.Run(rest, stdin, output, error),
            "length" => LengthCommand.Run(rest, stdin, output, error),
            "trim" => TrimCommand.Run(rest, stdin, output, error),
            "repeat" => RepeatCommand.Run(rest, stdin, output, error),
            "pad" => PadCommand.Run(rest, stdin, output, error),
            "sub" => SubCommand.Run(rest, stdin, output, error),
            "match" => MatchCommand.Run(rest, stdin, output, error),
            _ => UnknownCommand(command, error),
        };
    }

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string <command> [options] [STRING ...]");
        output.WriteLine();
        output.WriteLine("Commands:");
        output.WriteLine("  upper    Convert strings to uppercase");
        output.WriteLine("  lower    Convert strings to lowercase");
        output.WriteLine("  length   Print string lengths");
        output.WriteLine("  trim     Remove leading/trailing whitespace or characters");
        output.WriteLine("  repeat   Repeat strings");
        output.WriteLine("  pad      Pad strings to a fixed width");
        output.WriteLine("  sub      Extract substrings");
        output.WriteLine("  match    Match strings against a pattern");
        output.WriteLine();
        output.WriteLine("Use 'string <command> --help' for more information about a specific command.");
    }

    private static int UnknownCommand(string command, TextWriter error) {
        error.WriteLine($"error: unknown command '{command}'");
        return 1;
    }
}
