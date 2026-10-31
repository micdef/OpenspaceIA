# Test projects

One folder per test project. The settings and packages shared by every test project are in `test/Directory.Build.props`.

| Project | Tests |
|---------|-------|
| `OpenSpaceIA.Architecture.Tests` | The architecture rules: the Core depends on nothing but the .NET base library, and the Reader on the Core only (their project files and every type their code uses) |
| `OpenSpaceIA.Reader.Tests` | The base of the Reader: the allow-list of the files, and the safe reading of a file (missing, locked, refused, too large); the agent definitions (the forms of front matter read, and every file left out reported); the open sessions (the session files read, the closed sessions left out, the check of a process on the real system); the transcripts read line by line, and the sub-agent runs (their description, their summary, the cache, the window); the transcripts read only where they grew, and the reason of a wait (the open tool calls); the periods of the chief (turns, waits, interruptions); the snapshot that gathers every part (fixed clock, invented processes); on invented files in a temporary folder |
| `OpenSpaceIA.Core.Tests` | The core: every value of the model is checked when it is built (`Model/`); the state rules, with their thresholds (`States/`) |

## Running the tests

The tests run on **Microsoft.Testing.Platform** (MTP), selected in `global.json`.

| Tool | Command |
|------|---------|
| .NET CLI, every test project | `dotnet test --solution <solution>` |
| .NET CLI, one test project | `dotnet test --project test/<project>` |
| Visual Studio | Test Explorer |

## The local smoke test

`LocalSmokeTests` reads the real folder of Claude Code of the user (`~/.claude`) and writes a report: the sessions of the
chief and their state, each agent type with its runs and its state, the periods of the chief, the problems grouped by kind,
and the times. It writes no content of a file and writes nothing on the disk.

It is an **explicit** test: a normal run of the tests (and the CI) skips it, so that the files of the user are never read
without asking. To run it, alone, with its report:

```
dotnet test --project test/OpenSpaceIA.Reader.Tests -c Release --output detailed -- --explicit only --filter-class "OpenSpaceIA.Reader.Tests.LocalSmokeTests"
```

## Conventions

- A test project is named after the project it tests, followed by `.Tests`.
- Test framework: xUnit v3, with Shouldly for the assertions, and ArchUnitNET for the architecture rules.
- The core is tested with invented `~/.claude` files, never with real transcripts.
