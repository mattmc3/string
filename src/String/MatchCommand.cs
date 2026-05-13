using GetOpt;
using System.Text;
using System.Text.RegularExpressions;

public static class MatchCommand {
    private static readonly Getopt Parser = new("+haegirqvnm:", ["help", "all", "entire", "ignore-case", "groups-only", "index", "regex", "quiet", "invert", "max-matches="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string match [-h] [-a] [-e] [-i] [-g] [-n] [-r] [-q] [-v] [-m MAX] PATTERN [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Match STRING against PATTERN (glob by default).");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -a, --all              Find all matches (not just the first)");
        output.WriteLine("  -e, --entire           Print the entire STRING for matches");
        output.WriteLine("  -i, --ignore-case      Ignore case when matching");
        output.WriteLine("  -g, --groups-only      Print only capture groups (requires -r)");
        output.WriteLine("  -n, --index            Print match position and length instead of value");
        output.WriteLine("  -r, --regex            Treat PATTERN as a regular expression");
        output.WriteLine("  -v, --invert           Print strings that do NOT match");
        output.WriteLine("  -m, --max-matches MAX  Maximum number of matches to output");
        output.WriteLine("  -q, --quiet            Suppress output; exit 0 if any match, 1 if none");
        output.WriteLine("  -h, --help             Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool all = false, entire = false, ignoreCase = false, groupsOnly = false;
        bool useIndex = false, useRegex = false, quiet = false, invert = false;
        int maxMatches = int.MaxValue;

        var (opts, rest) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-a":
                case "--all":
                    all = true;
                    break;
                case "-e":
                case "--entire":
                    entire = true;
                    break;
                case "-i":
                case "--ignore-case":
                    ignoreCase = true;
                    break;
                case "-g":
                case "--groups-only":
                    groupsOnly = true;
                    break;
                case "-n":
                case "--index":
                    useIndex = true;
                    break;
                case "-r":
                case "--regex":
                    useRegex = true;
                    break;
                case "-q":
                case "--quiet":
                    quiet = true;
                    break;
                case "-v":
                case "--invert":
                    invert = true;
                    break;
                case "-m":
                case "--max-matches":
                    if (!int.TryParse(opt.Arg!, out maxMatches) || maxMatches <= 0) {
                        error.WriteLine($"error: match: Invalid max matches value '{opt.Arg}'");
                        return 1;
                    }
                    break;
            }
        }

        if (invert && groupsOnly) {
            error.WriteLine("error: match: invalid option combination, --invert and --groups-only are mutually exclusive");
            return 1;
        }
        if (entire && useIndex) {
            error.WriteLine("error: match: invalid option combination, --entire and --index are mutually exclusive");
            return 1;
        }
        if (entire && groupsOnly) {
            error.WriteLine("error: match: invalid option combination, --entire and --groups-only are mutually exclusive");
            return 1;
        }
        if (maxMatches == 0) {
            error.WriteLine($"error: match: Invalid max matches value '0'");
            return 1;
        }
        if (rest.Count == 0) {
            error.WriteLine("error: match requires a pattern");
            return 1;
        }

        Regex re;
        try {
            re = useRegex ? BuildRegex(rest[0], ignoreCase) : GlobToRegex(rest[0], ignoreCase, entire);
        }
        catch (ArgumentException ex) {
            error.WriteLine($"error: invalid pattern: {ex.Message}");
            return 1;
        }

        var strArgs = rest.Skip(1).Where(s => s != "--").ToList();
        IEnumerable<string> strings = CommandUtils.Strings(strArgs, stdin);
        bool changes = false;
        int totalMatches = 0;

        foreach (var s in strings) {
            if (invert) {
                if (!re.IsMatch(s)) {
                    if (totalMatches >= maxMatches) {
                        break;
                    }
                    if (!quiet) {
                        if (useIndex) {
                            output.WriteLine($"1 {s.Length}");
                        }
                        else {
                            output.WriteLine(s);
                        }
                    }
                    changes = true;
                    totalMatches++;
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
        if (!useIndex && match.Groups.Count > 1) {
            for (int g = 1; g < match.Groups.Count; g++) {
                output?.WriteLine(match.Groups[g].Value);
            }
        }
        return true;
    }

    private static Regex GlobToRegex(string glob, bool ignoreCase, bool entire = false) {
        var sb = new StringBuilder(entire ? "" : "^");
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
        if (!entire) {
            sb.Append('$');
        }
        RegexOptions options = RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        return new Regex(sb.ToString(), options);
    }

    private static readonly Regex Pcre2BackrefNum = new(@"\\g(\d+)", RegexOptions.Compiled);
    private static readonly Regex Pcre2BackrefBraces = new(@"\\g\{(\d+)\}", RegexOptions.Compiled);

    private static Regex BuildRegex(string pattern, bool ignoreCase) {
        pattern = Pcre2BackrefBraces.Replace(pattern, @"\$1");
        pattern = Pcre2BackrefNum.Replace(pattern, @"\$1");
        RegexOptions options = RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        return new Regex(pattern, options);
    }
}
