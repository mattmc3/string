# string

A .NET 10 CLI tool with Native AOT compilation.

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
just run
# or
dotnet run --project src/String/String.csproj
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

The compiled binary will be output as `string` (lowercase).

## Available Commands

Run `just` to see all available commands:

- `build` - Build the solution
- `run` - Run the console application
- `test` - Run tests
- `clean` - Clean build artifacts
- `restore` - Restore dependencies
- `publish` - Publish AOT binary
- `publish-native` - Publish self-contained native AOT
- `watch` - Watch and rebuild on changes
- `watch-test` - Watch and run tests on changes
- `update` - Update outdated packages

## Project Structure

- `src/String/` - Main console application
- `tests/String.Tests/` - xUnit v3 test project
- `String.slnx` - Solution file (modern XML format)

## Configuration

The project is configured for:
- Native AOT compilation (`PublishAot=true`)
- Invariant globalization for smaller binary size
- xUnit v3 with Microsoft Testing Platform
