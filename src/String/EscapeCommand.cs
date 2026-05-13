using GetOpt;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public static class EscapeCommand {
    private static readonly Getopt Parser = new("hn", ["help", "no-quoted", "style="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string escape [-h] [-n] [--style=STYLE] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Escape strings for safe use in various contexts.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -n, --no-quoted    Skip quoting strings that don't need it (script style only)");
        output.WriteLine("  --style=STYLE      script (default), url, html, regex, var");
        output.WriteLine("  -h, --help         Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        bool noQuoted = false;
        string style = "script";

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            switch (opt.Opt) {
                case "-n":
                case "--no-quoted":
                    noQuoted = true;
                    break;
                case "--style":
                    style = opt.Arg!;
                    break;
            }
        }

        if (!EscapeCore.ValidStyles.Contains(style)) {
            error.WriteLine($"error: escape: Invalid escape style '{style}'");
            return 1;
        }

        IEnumerable<string> strings = CommandUtils.Strings(inputs, stdin);
        bool any = false;

        foreach (var s in strings) {
            string? result = EscapeCore.Escape(s, style, noQuoted, error);
            if (result == null) {
                return 1;
            }
            output.WriteLine(result);
            any = true;
        }

        return any ? 0 : 1;
    }
}

public static class UnescapeCommand {
    private static readonly Getopt Parser = new("h", ["help", "style="]);

    public static void WriteHelp(TextWriter output) {
        output.WriteLine("Usage: string unescape [-h] [--style=STYLE] [STRING ...]");
        output.WriteLine();
        output.WriteLine("  Unescape strings from various encoded formats.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  --style=STYLE    script (default), url, html, regex, var");
        output.WriteLine("  -h, --help       Show this help message");
    }

    public static int Run(string[] args, TextReader stdin, TextWriter output, TextWriter error) {
        string style = "script";

        var (opts, inputs) = Parser.Parse(args);

        if (opts.Any(o => o.Opt is "-h" or "--help")) {
            WriteHelp(output);
            return 0;
        }

        foreach (var opt in opts) {
            if (opt.Opt == "--style") {
                style = opt.Arg!;
            }
        }

        if (!EscapeCore.ValidStyles.Contains(style)) {
            error.WriteLine($"error: unescape: Invalid style value '{style}'");
            return 1;
        }

        IEnumerable<string> strings = CommandUtils.Strings(inputs, stdin);
        bool any = false;

        foreach (var s in strings) {
            string? result = EscapeCore.Unescape(s, style, error);
            if (result == null) {
                return 1;
            }
            output.WriteLine(result);
            any = true;
        }

        return any ? 0 : 1;
    }
}

internal static class EscapeCore {
    private static readonly Regex VarPattern = new(@"_([0-9A-Fa-f]+)_", RegexOptions.Compiled);
    internal static readonly HashSet<string> ValidStyles = ["script", "url", "html", "regex", "var"];

    internal static string? Escape(string s, string style, bool noQuoted, TextWriter error) {
        if (!ValidStyles.Contains(style)) {
            error.WriteLine($"error: escape: Invalid escape style '{style}'");
            return null;
        }
        return style switch {
            "url" => Uri.EscapeDataString(s),
            "html" => WebUtility.HtmlEncode(s),
            "regex" => Regex.Escape(s),
            "var" => EscapeVar(s),
            _ => EscapeScript(s, noQuoted),
        };
    }

    internal static string? Unescape(string s, string style, TextWriter error) {
        if (!ValidStyles.Contains(style)) {
            error.WriteLine($"error: unescape: Invalid style value '{style}'");
            return null;
        }
        try {
            return style switch {
                "url" => Uri.UnescapeDataString(s),
                "html" => WebUtility.HtmlDecode(s),
                "regex" => Regex.Unescape(s),
                "var" => UnescapeVar(s),
                _ => UnescapeScript(s),
            };
        }
        catch (Exception ex) {
            error.WriteLine($"error: unescape failed: {ex.Message}");
            return null;
        }
    }

    private static string EscapeScript(string s, bool noQuoted) {
        bool safe = s.Length > 0 && s.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' or '/');
        if (safe) return noQuoted ? s : $"'{s}'";
        return noQuoted ? EscapeScriptNoQuoted(s) : EscapeScriptQuoted(s);
    }

    private static string EscapeScriptQuoted(string s) {
        var sb = new StringBuilder();
        bool inQuote = false;
        foreach (char c in s) {
            if (IsScriptControlChar(c)) {
                if (inQuote) { sb.Append('\''); inQuote = false; }
                sb.Append(EscapeControlChar(c));
            }
            else {
                if (!inQuote) { sb.Append('\''); inQuote = true; }
                if (c == '\'') sb.Append('\\');
                sb.Append(c);
            }
        }
        if (inQuote) sb.Append('\'');
        if (sb.Length == 0) sb.Append("''");
        return sb.ToString();
    }

    private static string EscapeScriptNoQuoted(string s) {
        var sb = new StringBuilder();
        char? prev = null;
        foreach (char c in s) {
            if (IsScriptControlChar(c)) {
                sb.Append(EscapeControlChar(c));
            }
            else if (c == '#' && (prev == null || prev == ' ' || prev == '\t')) {
                sb.Append("\\#");
            }
            else if (NeedsBackslashEscape(c)) {
                sb.Append('\\');
                sb.Append(c);
            }
            else {
                sb.Append(c);
            }
            prev = c;
        }
        return sb.ToString();
    }

    private static bool IsScriptControlChar(char c) => c < 0x20 || c == 0x7F;

    private static string EscapeControlChar(char c) {
        int n = (int)c;
        if (n >= 1 && n <= 26) return $"\\c{(char)('a' + n - 1)}";
        if (n == 27) return "\\e";
        if (n == 127) return "\\x7f";
        return $"\\x{n:x2}";
    }

    private static bool NeedsBackslashEscape(char c) =>
        c is ' ' or '\t' or '"' or '\'' or '\\' or '$' or '~'
            or '|' or '&' or ';' or '(' or ')' or '[' or ']'
            or '{' or '}' or '<' or '>';

    private static string UnescapeScript(string s) {
        var sb = new StringBuilder();
        int i = 0;
        while (i < s.Length) {
            if (s[i] == '\'') {
                i++;
                // Inside single-quoted strings, only \\ and \' are recognized escape sequences (fish behavior)
                while (i < s.Length && s[i] != '\'') {
                    if (s[i] == '\\' && i + 1 < s.Length && s[i + 1] is '\'' or '\\') {
                        sb.Append(s[i + 1]);
                        i += 2;
                    }
                    else {
                        sb.Append(s[i++]);
                    }
                }
                if (i < s.Length) {
                    i++;
                }
            }
            else if (s[i] == '\\' && i + 1 < s.Length) {
                sb.Append(s[i + 1]);
                i += 2;
            }
            else {
                sb.Append(s[i++]);
            }
        }
        return sb.ToString();
    }

    private static string EscapeVar(string s) {
        var sb = new StringBuilder();
        foreach (char c in s) {
            if (char.IsLetterOrDigit(c)) {
                sb.Append(c);
            }
            else if (c == '_') {
                sb.Append("__");
            }
            else {
                sb.Append($"_{(int)c:X2}_");
            }
        }
        return sb.ToString();
    }

    private static string UnescapeVar(string s) {
        var sb = new StringBuilder();
        int i = 0;
        while (i < s.Length) {
            if (s[i] == '_') {
                if (i + 1 < s.Length && s[i + 1] == '_') {
                    sb.Append('_');
                    i += 2;
                }
                else {
                    int j = i + 1;
                    while (j < s.Length && IsHexChar(s[j])) j++;
                    if (j > i + 1 && j < s.Length && s[j] == '_') {
                        sb.Append((char)Convert.ToInt32(s[(i + 1)..j], 16));
                        i = j + 1;
                    }
                    else {
                        sb.Append(s[i++]);
                    }
                }
            }
            else {
                sb.Append(s[i++]);
            }
        }
        return sb.ToString();
    }

    private static bool IsHexChar(char c) =>
        (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
}
