### Added

- Core project (`src/OpenSpaceIA.Core`): the immutable model of the office (machine, project, session of the chief, agent type, run of a sub-agent, task of a task list, snapshot of the activity) and its read port. Every value is checked when it is built (times in UTC, a reason for every waiting session, lists copied); the path of a project loses its trailing separators, so that one folder is always one project. The project depends on nothing.
- First test project (`test/OpenSpaceIA.Core.Tests`), with the shared test settings (`test/Directory.Build.props`): xUnit v3 on Microsoft.Testing.Platform, Shouldly, code coverage and TRX reports.
- Roslynator analyzers for every project, so that the build enforces the Roslynator rules of `.editorconfig`.

### Changed

- CI: the test step always runs; the temporary skip used while there was no test project is removed.
