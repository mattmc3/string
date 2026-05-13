using GetOpt;

public static class SplitCommand {
    public static void WriteHelp(TextWriter output) => SplitCore.WriteHelp("split", output);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        var (opts, rest) = SplitCore.Parser.Parse(args);
        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        if (rest.Count == 0) {
            error.WriteLine("error: split requires a separator");
            return 1;
        }

        return SplitCore.Run(rest[0], false, opts, rest.Skip(1).ToList(), stdin, output, error);
    }
}

public static class Split0Command {
    public static void WriteHelp(TextWriter output) => SplitCore.WriteHelp("split0", output);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        var (opts, inputs) = SplitCore.Parser.Parse(args);
        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }
        return SplitCore.Run("\0", true, opts, inputs, stdin, output, error);
    }
}

internal static class SplitCore {
    internal static readonly Getopt Parser = new("+hqnraf:m:", ["help", "quiet", "no-empty", "right", "allow-empty", "fields=", "max="]);

    internal static void WriteHelp(string name, TextWriter output) {
        bool nul0 = name == "split0";
        if (nul0) {
            output.WriteLine("Usage: string split0 [-h] [-n] [-r] [-q] [(-f | --fields) FIELDS [-a]] [(-m | --max) MAX] [STRING ...]");
        }
        else {
            output.WriteLine("Usage: string split [-h] [-n] [-r] [-q] [(-f | --fields) FIELDS [-a]] [(-m | --max) MAX] SEP [STRING ...]");
        }
        output.WriteLine();
        output.WriteLine(nul0
            ? "  Split each STRING by NUL (\\0). Trailing NUL is ignored."
            : "  Split each STRING by SEP.");
        output.WriteLine();
        output.WriteLine("Options:");
        if (!nul0) {
            output.WriteLine("  SEP                   Separator string");
        }
        output.WriteLine("  -n, --no-empty        Suppress empty results");
        output.WriteLine("  -r, --right           Split from the right (useful with --max)");
        output.WriteLine("  -f, --fields FIELDS   Output only specified fields (e.g. 1,3-5)");
        output.WriteLine("  -a, --allow-empty     With --fields, skip missing fields instead of failing");
        output.WriteLine("  -m, --max MAX         Maximum number of splits per string");
        output.WriteLine("  -q, --quiet           Suppress output; exit 0 if any splits, 1 if none");
        output.WriteLine("  -h, --help            Show this help message");
    }

    internal static int Run(string sep, bool nul0Mode, IReadOnlyList<OptArg> opts, IReadOnlyList<string> inputs, TextReader stdin, TextWriter output, TextWriter error) {
        bool quiet = false, noEmpty = false, right = false, allowEmpty = false;
        int max = 0;
        int[]? fields = null;

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-q": case "--quiet": quiet = true; break;
                case "-n": case "--no-empty": noEmpty = true; break;
                case "-r": case "--right": right = true; break;
                case "-a": case "--allow-empty": allowEmpty = true; break;
                case "-m": case "--max": max = int.Parse(opt.Arg!); break;
                case "-f":
                case "--fields":
                    fields = ParseFields(opt.Arg!, error);
                    if (fields == null) {
                        return 1;
                    }
                    break;
            }
        }

        IEnumerable<string> strings = inputs.Count > 0
            ? (nul0Mode ? inputs.Select(s => s.TrimEnd('\0')) : inputs)
            : ReadStrings(stdin, nul0Mode);

        bool anySplit = false;
        bool allFieldsFound = true;

        foreach (var s in strings) {
            var parts = right ? SplitRight(s, sep, max) : SplitLeft(s, sep, max);
            if (parts.Count > 1) {
                anySplit = true;
            }
            if (noEmpty) {
                parts = parts.Where(p => p.Length > 0).ToList();
            }

            IEnumerable<string> selected;
            if (fields != null) {
                var (fieldItems, ok) = SelectFields(parts, fields, allowEmpty);
                selected = fieldItems;
                if (!ok) {
                    allFieldsFound = false;
                }
            }
            else {
                selected = parts;
            }

            foreach (var part in selected) {
                if (!quiet) {
                    output.WriteLine(part);
                }
            }
        }

        return (anySplit && allFieldsFound) ? 0 : 1;
    }

    private static IEnumerable<string> ReadStrings(TextReader reader, bool nul0Mode) {
        if (!nul0Mode) {
            return CommandUtils.ReadLines(reader);
        }
        var content = reader.ReadToEnd();
        if (content.EndsWith('\0')) {
            content = content[..^1];
        }
        return [content];
    }

    private static List<string> SplitLeft(string s, string sep, int max) {
        if (sep.Length == 0) {
            return s.Length == 0 ? [""] : [.. s.Select(c => c.ToString())];
        }
        if (max > 0) {
            return [.. s.Split([sep], max + 1, StringSplitOptions.None)];
        }
        return [.. s.Split([sep], StringSplitOptions.None)];
    }

    private static List<string> SplitRight(string s, string sep, int max) {
        if (sep.Length == 0) {
            var chars = s.Length == 0 ? (List<string>)[""] : [.. s.Select(c => c.ToString())];
            if (max > 0 && chars.Count > max + 1) {
                string head = string.Concat(chars.Take(chars.Count - max));
                return [head, .. chars.Skip(chars.Count - max)];
            }
            return chars;
        }
        var parts = new List<string>();
        int count = 0;
        string remaining = s;
        while (true) {
            int idx = remaining.LastIndexOf(sep, StringComparison.Ordinal);
            if (idx < 0 || (max > 0 && count >= max)) {
                parts.Insert(0, remaining);
                break;
            }
            parts.Insert(0, remaining[(idx + sep.Length)..]);
            remaining = remaining[..idx];
            count++;
        }
        return parts;
    }

    private static (List<string> items, bool allFound) SelectFields(List<string> parts, int[] fields, bool allowEmpty) {
        var items = new List<string>();
        bool allFound = true;
        foreach (int f in fields) {
            if (f >= 1 && f <= parts.Count) {
                items.Add(parts[f - 1]);
            }
            else if (!allowEmpty) {
                allFound = false;
            }
        }
        return (items, allFound);
    }

    private static int[]? ParseFields(string spec, TextWriter error) {
        var fields = new List<int>();
        foreach (var part in spec.Split(',')) {
            int dashIdx = part.IndexOf('-', 1);
            if (dashIdx > 0) {
                if (!int.TryParse(part[..dashIdx], out int from) || !int.TryParse(part[(dashIdx + 1)..], out int to)) {
                    error.WriteLine($"error: invalid field spec: {part}");
                    return null;
                }
                for (int i = from; i <= to; i++) {
                    fields.Add(i);
                }
            }
            else {
                if (!int.TryParse(part, out int f)) {
                    error.WriteLine($"error: invalid field spec: {part}");
                    return null;
                }
                fields.Add(f);
            }
        }
        return [.. fields];
    }
}
