internal static class CommandUtils {
    internal static IEnumerable<string> ReadLines(TextReader reader) {
        string? line;
        while ((line = reader.ReadLine()) != null) {
            yield return line;
        }
    }

    internal static IEnumerable<string> Strings(IReadOnlyList<string> inputs, TextReader stdin) =>
        inputs.Count > 0 ? inputs : ReadLines(stdin);

    internal static IReadOnlyList<string> StringsList(IReadOnlyList<string> inputs, TextReader stdin) =>
        inputs.Count > 0 ? inputs : ReadLines(stdin).ToList();
}

internal static class VisualWidth {
    // Skip past an ANSI escape sequence; i points at \x1b on entry.
    private static int SkipAnsi(string s, int i) {
        i++; // skip \x1b
        if (i < s.Length && s[i] == '[') {
            i++;
            while (i < s.Length && !char.IsLetter(s[i])) i++;
            if (i < s.Length) i++;
        }
        else if (i < s.Length) {
            i++;
        }
        return i;
    }

    // Visual width of s on a single line: ANSI-aware, backspace, control chars zero-width.
    // Does NOT split on \n or reset on \r.
    internal static int Of(string s) {
        int w = 0, i = 0;
        while (i < s.Length) {
            char c = s[i];
            if (c == '\x1b') {
                i = SkipAnsi(s, i);
            }
            else if (c == '\b') {
                w = Math.Max(0, w - 1);
                i++;
            }
            else if (char.IsControl(c)) {
                i++;
            }
            else {
                w++;
                i++;
            }
        }
        return w;
    }

    // Yield visual width for each line in s (splits on \n, handles \r).
    internal static IEnumerable<int> OfLines(string s) {
        int pos = 0, i = 0;
        while (i < s.Length) {
            char c = s[i];
            if (c == '\n') {
                yield return pos;
                pos = 0;
                i++;
            }
            else if (c == '\r') {
                pos = 0;
                i++;
            }
            else if (c == '\x1b') {
                i = SkipAnsi(s, i);
            }
            else if (c == '\b') {
                pos = Math.Max(0, pos - 1);
                i++;
            }
            else if (char.IsControl(c)) {
                i++;
            }
            else {
                pos++;
                i++;
            }
        }
        yield return pos;
    }

    // Take chars from left of s with cumulative visual width == targetWidth.
    internal static string TakeLeft(string s, int targetWidth) {
        int w = 0, i = 0, cutAt = 0;
        while (i < s.Length) {
            char c = s[i];
            if (c == '\x1b') {
                int end = SkipAnsi(s, i);
                if (w < targetWidth) cutAt = end;
                i = end;
            }
            else if (c == '\b') {
                w = Math.Max(0, w - 1);
                if (w < targetWidth) cutAt = i + 1;
                i++;
            }
            else if (char.IsControl(c)) {
                if (w < targetWidth) cutAt = i + 1;
                i++;
            }
            else {
                if (w >= targetWidth) break;
                w++;
                i++;
                cutAt = i;
            }
        }
        return s[..cutAt];
    }

    // Take chars from right of s with cumulative visual width == targetWidth.
    internal static string TakeRight(string s, int targetWidth) {
        var segs = new List<(int Start, int End, int Width)>();
        int pos = 0;
        while (pos < s.Length) {
            char c = s[pos];
            if (c == '\x1b') {
                int end = SkipAnsi(s, pos);
                segs.Add((pos, end, 0));
                pos = end;
            }
            else if (c == '\b') {
                segs.Add((pos, pos + 1, -1));
                pos++;
            }
            else if (char.IsControl(c)) {
                segs.Add((pos, pos + 1, 0));
                pos++;
            }
            else {
                segs.Add((pos, pos + 1, 1));
                pos++;
            }
        }
        int w = 0;
        for (int j = segs.Count - 1; j >= 0; j--) {
            int sw = segs[j].Width;
            if (sw <= 0) continue;
            if (w >= targetWidth) return s[segs[j].End..];
            w += sw;
        }
        return s;
    }
}
