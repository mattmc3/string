namespace GetOpt;

public record OptArg(string Opt, string? Arg = null);

public class Getopt {
    private enum ArgMode { None, Required, Optional }

    private readonly Dictionary<char, ArgMode> _shortMap;
    private readonly Dictionary<string, bool> _longMap;
    private readonly bool _posixMode;

    public Getopt(string shortOpts = "", IEnumerable<string>? longOpts = null) {
        _posixMode = shortOpts.StartsWith('+');
        _shortMap = ParseShortSpec(_posixMode ? shortOpts[1..] : shortOpts);
        _longMap = ParseLongSpec(longOpts ?? []);
    }

    public (IReadOnlyList<OptArg> opts, IReadOnlyList<string> args) Parse(string[] args) {
        var opts = new List<OptArg>();
        var remaining = new List<string>();
        int i = 0;

        while (i < args.Length) {
            var arg = args[i];

            if (arg == "--") {
                for (int j = i + 1; j < args.Length; j++) {
                    remaining.Add(args[j]);
                }
                break;
            }
            else if (arg.StartsWith("--") && arg.Length > 2) {
                var name = arg[2..];
                string? value = null;
                var eq = name.IndexOf('=');
                if (eq >= 0) {
                    value = name[(eq + 1)..];
                    name = name[..eq];
                }
                if (!_longMap.TryGetValue(name, out bool takesArg)) {
                    throw new ArgumentException($"unknown option: --{name}");
                }
                if (takesArg && value is null) {
                    if (++i >= args.Length) {
                        throw new ArgumentException($"option --{name} requires an argument");
                    }
                    value = args[i];
                }
                opts.Add(new OptArg($"--{name}", value));
                i++;
            }
            else if (arg.StartsWith('-') && arg.Length > 1) {
                int j = 1;
                while (j < arg.Length) {
                    char c = arg[j];
                    if (!_shortMap.TryGetValue(c, out ArgMode mode)) {
                        throw new ArgumentException($"unknown option: -{c}");
                    }
                    if (mode == ArgMode.Required) {
                        string value;
                        if (j + 1 < arg.Length) {
                            value = arg[(j + 1)..];
                            j = arg.Length;
                        }
                        else {
                            if (++i >= args.Length) {
                                throw new ArgumentException($"option -{c} requires an argument");
                            }
                            value = args[i];
                            j++;
                        }
                        opts.Add(new OptArg($"-{c}", value));
                    }
                    else if (mode == ArgMode.Optional) {
                        string? value = j + 1 < arg.Length ? arg[(j + 1)..] : null;
                        j = arg.Length;
                        opts.Add(new OptArg($"-{c}", value));
                    }
                    else {
                        opts.Add(new OptArg($"-{c}"));
                        j++;
                    }
                }
                i++;
            }
            else {
                remaining.Add(arg);
                if (_posixMode) {
                    for (int j = i + 1; j < args.Length; j++) {
                        remaining.Add(args[j]);
                    }
                    break;
                }
                i++;
            }
        }

        return (opts, remaining);
    }

    private static Dictionary<char, ArgMode> ParseShortSpec(string spec) {
        var map = new Dictionary<char, ArgMode>();
        int i = 0;
        while (i < spec.Length) {
            char c = spec[i];
            if (i + 2 < spec.Length && spec[i + 1] == ':' && spec[i + 2] == ':') {
                map[c] = ArgMode.Optional;
                i += 3;
            }
            else if (i + 1 < spec.Length && spec[i + 1] == ':') {
                map[c] = ArgMode.Required;
                i += 2;
            }
            else {
                map[c] = ArgMode.None;
                i++;
            }
        }
        return map;
    }

    private static Dictionary<string, bool> ParseLongSpec(IEnumerable<string> specs) {
        var map = new Dictionary<string, bool>();
        foreach (var spec in specs) {
            if (spec.EndsWith('=')) {
                map[spec[..^1]] = true;
            }
            else {
                map[spec] = false;
            }
        }
        return map;
    }
}
