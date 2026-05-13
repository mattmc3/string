namespace String.Tests;

public abstract class TestBase {
    protected static (int exitCode, string stdout, string stderr) Run(params string[] args) {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var exit = StringApp.Run(args, stdout, stderr);
        return (exit, stdout.ToString(), stderr.ToString());
    }

    protected static (int exitCode, string stdout, string stderr) RunWithStdin(string stdinContent, params string[] args) {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var stdin = new StringReader(stdinContent);
        var exit = StringApp.Run(args, stdin, stdout, stderr);
        return (exit, stdout.ToString(), stderr.ToString());
    }

    protected static string[] Lines(string stdout) =>
        stdout.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
}
