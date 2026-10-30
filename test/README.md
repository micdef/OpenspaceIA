# Test projects

One folder per test project. No project yet.

## Running the tests

The tests run on **Microsoft.Testing.Platform** (MTP), selected in `global.json`.

| Tool | Command |
|------|---------|
| .NET CLI, every test project | `dotnet test --solution <solution>` |
| .NET CLI, one test project | `dotnet test --project test/<project>` |
| Visual Studio | Test Explorer |

## Conventions

- A test project is named after the project it tests, followed by `.Tests`.
- Test framework: xUnit v3, with Shouldly for the assertions, and ArchUnitNET for the architecture rules.
- The core is tested with invented `~/.claude` files, never with real transcripts.
