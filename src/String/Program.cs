return StringApp.Run(args, Console.Out, Console.Error);

public static class StringApp {
    public static int Run(string[] args, TextWriter output, TextWriter error) {
        if (args.Length < 2) {
            error.WriteLine("Usage: string <command> <args...>");
            error.WriteLine("Commands: upper, lower");
            return 1;
        }

        var command = args[0];
        var inputs = args[1..];

        foreach (var s in inputs) {
            output.WriteLine(command switch {
                "upper" => s.ToUpperInvariant(),
                "lower" => s.ToLowerInvariant(),
                _ => throw new Exception($"Unknown command: {command}")
            });
        }

        return 0;
    }
}
