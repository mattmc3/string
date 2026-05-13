return StringApp.Run(args, Console.In, Console.Out, Console.Error);

public static class StringApp {
    public static int Run(string[] args, TextWriter output, TextWriter error) =>
        Run(args, TextReader.Null, output, error);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        if (args.Length < 1) {
            error.WriteLine("string: missing subcommand");
            return 1;
        }

        var command = args[0];
        if (command is "--help" or "-h" or "help") {
            WriteHelp(output);
            return 0;
        }

        var rest = args[1..];

        try {
            return command switch {
                "upper" => UpperCommand.Run(rest, stdin, output, error),
                "lower" => LowerCommand.Run(rest, stdin, output, error),
                "length" => LengthCommand.Run(rest, stdin, output, error),
                "trim" => TrimCommand.Run(rest, stdin, output, error),
                "repeat" => RepeatCommand.Run(rest, stdin, output, error),
                "pad" => PadCommand.Run(rest, stdin, output, error),
                "sub" => SubCommand.Run(rest, stdin, output, error),
                "shorten" => ShortenCommand.Run(rest, stdin, output, error),
                "replace" => ReplaceCommand.Run(rest, stdin, output, error),
                "split" => SplitCommand.Run(rest, stdin, output, error),
                "split0" => Split0Command.Run(rest, stdin, output, error),
                "join" => JoinCommand.Run(rest, stdin, output, error),
                "join0" => Join0Command.Run(rest, stdin, output, error),
                "match" => MatchCommand.Run(rest, stdin, output, error),
                "collect" => CollectCommand.Run(rest, stdin, output, error),
                "escape" => EscapeCommand.Run(rest, stdin, output, error),
                "unescape" => UnescapeCommand.Run(rest, stdin, output, error),
                _ => UnknownCommand(command, error),
            };
        }
        catch (ArgumentException ex) {
            error.WriteLine($"error: {ex.Message}");
            return 1;
        }
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
        output.WriteLine("  shorten  Shorten strings to a fixed width with ellipsis");
        output.WriteLine("  replace  Replace substrings");
        output.WriteLine("  split    Split strings by delimiter");
        output.WriteLine("  split0   Split strings by NUL");
        output.WriteLine("  join     Join strings with delimiter");
        output.WriteLine("  join0    Join strings with NUL");
        output.WriteLine("  match    Match strings against a pattern");
        output.WriteLine("  collect  Collect strings into a single output");
        output.WriteLine("  escape   Escape strings for use in various contexts");
        output.WriteLine("  unescape Unescape strings from various encoded formats");
        output.WriteLine();
        output.WriteLine("Use 'string <command> --help' for more information about a specific command.");
    }

    private static int UnknownCommand(string command, TextWriter error) {
        error.WriteLine($"string {command}: invalid subcommand");
        return 1;
    }
}
