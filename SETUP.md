# String Project Setup

## Commands to Execute

### 1. Create Solution File
```bash
dotnet new sln -n String
```

### 2. Create Console CLI Project
```bash
dotnet new console -n String -o src/String -f net10.0
```

### 3. Configure AOT Settings in Main Project
Edit `src/String/String.csproj` to add:
```xml
<AssemblyName>string</AssemblyName>
<PublishAot>true</PublishAot>
<InvariantGlobalization>true</InvariantGlobalization>
```

### 4. Install xUnit v3 Templates
```bash
dotnet new install xunit.v3.templates
```

### 5. Create xUnit3 Test Project
```bash
dotnet new xunit3 -n String.Tests -o tests/String.Tests -f net10.0
```

### 6. Add Projects to Solution
```bash
dotnet sln add src/String/String.csproj
dotnet sln add tests/String.Tests/String.Tests.csproj
```

### 7. Add Project Reference (Test -> Main)
```bash
dotnet add tests/String.Tests/String.Tests.csproj reference src/String/String.csproj
```

### 8. Update Packages (Optional)
```bash
dotnet outdated --upgrade
```

## Notes
- Solution file will be in .slnx format (modern XML-based solution format)
- Main project configured for Native AOT compilation
- Test project using xUnit v3
- Binary output name will be lowercase `string` instead of `String`
