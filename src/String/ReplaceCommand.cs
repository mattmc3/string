using GetOpt;
using System.Text;
using System.Text.RegularExpressions;

public static class ReplaceCommand {
    private static readonly Getopt Parser = new("+hafirqm:", ["help", "all", "filter", "ignore-case", "regex", "quiet", "max-matches="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string replace [-h] [-a] [-f] [-i] [-r] [-q] [(-m | --max-matches) MAX] PATTERN REPLACEMENT [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Replace PATTERN with REPLACEMENT in each STRING.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -a, --all                Replace all occurrences (default: first only)");
        output.WriteLine("  -f, --filter             Only print strings where a replacement was made");
        output.WriteLine("  -i, --ignore-case        Case-insensitive matching");
        output.WriteLine("  -r, --regex              Treat PATTERN as a regular expression");
        output.WriteLine("  -m, --max-matches MAX    Maximum number of replacements per string");
        output.WriteLine("  -q, --quiet              Suppress output; exit 0 if any replacement, 1 if none");
        output.WriteLine("  -h, --help               Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool all = false, filter = false, ignoreCase = false, useRegex = false, quiet = false;
        int maxMatches = 0;

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
                case "-f":
                case "--filter":
                    filter = true;
                    break;
                case "-i":
                case "--ignore-case":
                    ignoreCase = true;
                    break;
                case "-r":
                case "--regex":
                    useRegex = true;
                    break;
                case "-q":
                case "--quiet":
                    quiet = true;
                    break;
                case "-m":
                case "--max-matches":
                    if (!int.TryParse(opt.Arg!, out maxMatches)) {
                        error.WriteLine($"error: replace: Invalid max matches value '{opt.Arg}'");
                        return 1;
                    }
                    break;
            }
        }

        if (maxMatches < 0) {
            error.WriteLine($"error: replace: Invalid max matches value '{maxMatches}'");
            return 1;
        }

        if (rest.Count < 2) {
            error.WriteLine("error: replace requires a pattern and replacement");
            return 1;
        }

        string pattern = rest[0];
        string replacement = rest[1];
        int limit = maxMatches > 0 ? maxMatches : (all ? int.MaxValue : 1);

        Regex? re = null;
        if (useRegex) {
            try {
                var options = RegexOptions.None | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                re = new Regex(pattern, options);
            }
            catch (ArgumentException ex) {
                error.WriteLine($"error: invalid pattern: {ex.Message}");
                return 1;
            }
        }

        IEnumerable<string> strings = rest.Count > 2 ? rest.Skip(2) : CommandUtils.ReadLines(stdin); // can't use Strings(); rest has leading pattern args
        bool changes = false;

        foreach (var s in strings) {
            string result = re != null
                ? ReplaceRegex(s, re, replacement, limit)
                : ReplaceLiteral(s, pattern, replacement, limit, ignoreCase);

            bool changed = result != s;
            if (changed) {
                changes = true;
            }

            if (!filter || changed) {
                if (!quiet) {
                    output.WriteLine(result);
                }
            }
        }

        return changes ? 0 : 1;
    }

    private static string ReplaceLiteral(string s, string pattern, string replacement, int max, bool ignoreCase) {
        if (pattern.Length == 0) {
            return s;
        }
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var sb = new StringBuilder();
        int count = 0;
        int start = 0;
        while (count < max) {
            int idx = s.IndexOf(pattern, start, comparison);
            if (idx < 0) {
                break;
            }
            sb.Append(s, start, idx - start);
            sb.Append(replacement);
            start = idx + pattern.Length;
            count++;
        }
        sb.Append(s, start, s.Length - start);
        return sb.ToString();
    }

    private static string ReplaceRegex(string s, Regex re, string replacement, int max) {
        int count = 0;
        return re.Replace(s, m => {
            if (count >= max) {
                return m.Value;
            }
            count++;
            return m.Result(replacement);
        });
    }
}
