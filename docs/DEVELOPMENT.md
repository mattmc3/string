# Development

## Requirements

- .NET 10 SDK or later
- [just](https://github.com/casey/just) (optional, for running commands)

## Building

```bash
just build
# or
dotnet build
```

## Running

```bash
just run -- <command> [args]
# or
dotnet run --project src/String/String.csproj -- <command> [args]
```

## Testing

```bash
just test
# or
dotnet test
```

## Publishing Native AOT Binary

```bash
just publish-native
# or
dotnet publish src/String/String.csproj -c Release --self-contained
```

The compiled binary will be output as `publish/string`.

## Project Structure

- `src/String/` — Main console application
- `src/GetOpt/` — Lightweight option parser
- `tests/String.Tests/` — xUnit v3 test project
- `String.slnx` — Solution file (modern XML format)

## Configuration

- Native AOT compilation (`PublishAot=true`)
- Invariant globalization for smaller binary size
- xUnit v3 with Microsoft Testing Platform
