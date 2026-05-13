internal static class CommandUtils {
    internal static IEnumerable<string> ReadLines(TextReader reader) {
        string? line;
        while ((line = reader.ReadLine()) != null) {
            yield return line;
        }
    }
}
