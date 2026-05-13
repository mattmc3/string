# Default recipe to display help information
default:
    @just --list

# Build the solution
build:
    dotnet build

# Run the console application
run *ARGS:
    dotnet run --project src/String/String.csproj -- {{ARGS}}

# Run the tests
test *ARGS:
    dotnet test {{ARGS}}

# Clean build artifacts
clean:
    dotnet clean
    rm -rf src/String/bin src/String/obj
    rm -rf tests/String.Tests/bin tests/String.Tests/obj

# Restore dependencies
restore:
    dotnet restore

# Publish AOT binary
publish:
    dotnet publish src/String/String.csproj -c Release

# Publish AOT binary for current platform
publish-native:
    dotnet publish src/String/String.csproj -c Release --self-contained

# Watch and rebuild on changes
watch:
    dotnet watch --project src/String/String.csproj

# Watch and run tests on changes
watch-test:
    dotnet watch --project tests/String.Tests/String.Tests.csproj test

# Run hyperfine benchmarks against the native binary (optional filter: just benchmark match)
benchmark filter="":
    bin/benchmark {{filter}}

# Format source code
format:
    dotnet format

# Update outdated packages
update:
    dotnet outdated --upgrade
