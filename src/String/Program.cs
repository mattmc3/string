using GetOpt;
using System.Text;
using System.Text.RegularExpressions;

return StringApp.Run(args, Console.In, Console.Out, Console.Error);

public static class StringApp {
    public static int Run(string[] args, TextWriter output, TextWriter error) =>
        Run(args, TextReader.Null, output, error);

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        if (args.Length < 1) {
            error.WriteLine("Usage: string <command> [options] [STRING ...]");
            error.WriteLine("Commands: upper, lower, trim, repeat, match");
            return 1;
        }

        var command = args[0];
        var rest = args[1..];

        return command switch {
            "upper" => RunSimple(rest, stdin, s => s.ToUpperInvariant(), output, error),
            "lower" => RunSimple(rest, stdin, s => s.ToLowerInvariant(), output, error),
            "trim" => RunTrim(rest, stdin, output, error),
            "repeat" => RunRepeat(rest, stdin, output, error),
            "match" => RunMatch(rest, stdin, output, error),
            _ => UnknownCommand(command, error),
        };
    }

    private static IEnumerable<string> ReadLines(TextReader reader) {
        string? line;
        while ((line = reader.ReadLine()) != null) {
            yield return line;
        }
    }

    private static readonly Getopt SimpleParser = new("q", ["quiet"]);

    private static int RunSimple(string[] args, TextReader stdin, Func<string, string> transform, TextWriter output, TextWriter error) {
        var (opts, inputs) = SimpleParser.Parse(args);
        bool quiet = opts.Any(o => o.Opt is "-q" or "--quiet");
        IEnumerable<string> strings = inputs.Count > 0 ? inputs : ReadLines(stdin);
        bool changes = false;
        foreach (var s in strings) {
            var result = transform(s);
            if (!quiet) {
                output.WriteLine(result);
            }
            if (result != s) {
                changes = true;
            }
        }
        return changes ? 0 : 1;
    }

    private static int RunTrim(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool left = false, right = false, quiet = false;
        string? chars = null;

        var (opts, inputs) = new Getopt("+lrqc:", ["left", "right", "quiet", "chars="]).Parse(args);

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-l": case "--left": left = true; break;
                case "-r": case "--right": right = true; break;
                case "-q": case "--quiet": quiet = true; break;
                case "-c": case "--chars": chars = opt.Arg; break;
            }
        }

        if (!left && !right) {
            left = true;
            right = true;
        }

        IEnumerable<string> strings = inputs.Count > 0 ? inputs : ReadLines(stdin);
        bool changes = false;
        foreach (var s in strings) {
            var result = chars is null
                ? Trim(s, left, right)
                : Trim(s, left, right, chars.ToCharArray());
            if (!quiet) {
                output.WriteLine(result);
            }
            if (result.Length < s.Length) {
                changes = true;
            }
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

    private static readonly Getopt RepeatParser = new("n:m:Nq", ["count=", "max=", "no-newline", "quiet"]);

    private static int RunRepeat(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        int count = 0;
        int max = -1;
        bool noNewline = false;
        bool quiet = false;

        var (opts, inputs) = RepeatParser.Parse(args);

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-n": case "--count": count = int.Parse(opt.Arg!); break;
                case "-m": case "--max": max = int.Parse(opt.Arg!); break;
                case "-N": case "--no-newline": noNewline = true; break;
                case "-q": case "--quiet": quiet = true; break;
            }
        }

        if (count <= 0) {
            return 1;
        }

        IReadOnlyList<string> strings = inputs.Count > 0 ? inputs : ReadLines(stdin).ToList();
        if (strings.Count == 0) {
            return 1;
        }

        bool changes = false;
        for (int i = 0; i < strings.Count; i++) {
            var repeated = string.Concat(Enumerable.Repeat(strings[i], count));
            if (max >= 0 && repeated.Length > max) {
                repeated = repeated[..max];
            }
            if (repeated.Length > 0) {
                changes = true;
            }
            if (!quiet) {
                bool isLast = i == strings.Count - 1;
                if (noNewline && isLast) {
                    output.Write(repeated);
                }
                else {
                    output.WriteLine(repeated);
                }
            }
        }

        return changes ? 0 : 1;
    }

    private static readonly Getopt MatchParser = new("+aeginrqvm:", ["all", "entire", "ignore-case", "groups-only", "index", "regex", "quiet", "invert", "max-matches="]);

    private static int RunMatch(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool all = false, entire = false, ignoreCase = false, groupsOnly = false;
        bool useIndex = false, useRegex = false, quiet = false, invert = false;
        int maxMatches = int.MaxValue;

        var (opts, rest) = MatchParser.Parse(args);

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-a": case "--all": all = true; break;
                case "-e": case "--entire": entire = true; break;
                case "-i": case "--ignore-case": ignoreCase = true; break;
                case "-g": case "--groups-only": groupsOnly = true; break;
                case "-n": case "--index": useIndex = true; break;
                case "-r": case "--regex": useRegex = true; break;
                case "-q": case "--quiet": quiet = true; break;
                case "-v": case "--invert": invert = true; break;
                case "-m": case "--max-matches": maxMatches = int.Parse(opt.Arg!); break;
            }
        }

        if (rest.Count == 0) {
            error.WriteLine("error: match requires a pattern");
            return 1;
        }

        Regex re;
        try {
            re = useRegex ? BuildRegex(rest[0], ignoreCase) : GlobToRegex(rest[0], ignoreCase);
        }
        catch (ArgumentException ex) {
            error.WriteLine($"error: invalid pattern: {ex.Message}");
            return 1;
        }

        IEnumerable<string> strings = rest.Count > 1 ? rest.Skip(1) : ReadLines(stdin);
        bool changes = false;
        int totalMatches = 0;

        foreach (var s in strings) {
            if (invert) {
                if (!re.IsMatch(s)) {
                    if (!quiet) {
                        output.WriteLine(s);
                    }
                    changes = true;
                }
                continue;
            }

            if (all) {
                foreach (Match match in re.Matches(s)) {
                    if (totalMatches >= maxMatches) {
                        break;
                    }
                    bool had = WriteMatch(quiet ? null : output, match, s, groupsOnly, useIndex, entire);
                    if (had) {
                        changes = true;
                        totalMatches++;
                    }
                }
            }
            else {
                Match match = re.Match(s);
                if (match.Success && totalMatches < maxMatches) {
                    bool had = WriteMatch(quiet ? null : output, match, s, groupsOnly, useIndex, entire);
                    if (had) {
                        changes = true;
                        totalMatches++;
                    }
                }
            }

            if (totalMatches >= maxMatches) {
                break;
            }
        }

        return changes ? 0 : 1;
    }

    private static bool WriteMatch(TextWriter? output, Match match, string source, bool groupsOnly, bool useIndex, bool entire) {
        if (groupsOnly) {
            bool any = false;
            for (int g = 1; g < match.Groups.Count; g++) {
                Group group = match.Groups[g];
                if (!group.Success) {
                    continue;
                }
                output?.WriteLine(useIndex ? $"{group.Index + 1} {group.Length}" : group.Value);
                any = true;
            }
            return any;
        }

        output?.WriteLine(useIndex ? $"{match.Index + 1} {match.Length}" : entire ? source : match.Value);
        return true;
    }

    private static Regex GlobToRegex(string glob, bool ignoreCase) {
        var sb = new StringBuilder("^");
        int i = 0;
        while (i < glob.Length) {
            switch (glob[i]) {
                case '*':
                    sb.Append(".*");
                    i++;
                    break;
                case '?':
                    sb.Append('.');
                    i++;
                    break;
                case '[':
                    sb.Append('[');
                    i++;
                    if (i < glob.Length && glob[i] == '!') {
                        sb.Append('^');
                        i++;
                    }
                    while (i < glob.Length && glob[i] != ']') {
                        sb.Append(glob[i++]);
                    }
                    if (i < glob.Length) {
                        sb.Append(']');
                        i++;
                    }
                    break;
                default:
                    sb.Append(Regex.Escape(glob[i].ToString()));
                    i++;
                    break;
            }
        }
        sb.Append('$');
        RegexOptions options = RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        return new Regex(sb.ToString(), options);
    }

    private static Regex BuildRegex(string pattern, bool ignoreCase) {
        RegexOptions options = RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        return new Regex(pattern, options);
    }

    private static int UnknownCommand(string command, TextWriter error) {
        error.WriteLine($"error: unknown command '{command}'");
        return 1;
    }
}
