# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0] - 2026-10-30

The Core: the immutable model of the office and the rules that judge the workload of the agents, written first as tests, with no dependency; no user interface yet.

### Added

- State definitions (`doc/analyse/state-definitions.md`): an agent is an agent type, and the chief is one of them; the state at a given moment (working, waiting for you with its reason, idle); overloaded (over the last 2 hours, at least 2 projects and no break of 10 minutes or more); under-used (idle at least 90 % of the last 7 days); every value is a default that the user can change. Then the periods given to the rules, the merge of the periods that overlap, and how the thresholds are read (the start and the end of the window count as breaks; a period counts only for a part of more than zero inside the window).
- Coding conventions (`doc/dev/coding-conventions.md`): file headers, usings, regions, documentation of every member, step comments and code rules for C#; and the tests, named in PascalCase by the rule they prove, with "now" as a parameter and invented data only.
- Doxygen configuration (`Doxyfile` at the root): the HTML documentation of `src/`, with the README as main page and the contribution guide, the changelog, the analysis documents and the coding conventions; private members are published too, since they are documented. The output in `doc/dev/html/` is not versioned.
- Core project (`src/OpenSpaceIA.Core`): the immutable model of the office (machine, project, session of the chief, agent type, run of a sub-agent, task of a task list, period of work or of waiting, snapshot of the activity) and its read port. Every value is checked when it is built (times in UTC, a reason for every waiting session, lists copied); the path of a project loses its trailing separators, so that one folder is always one project. The project depends on nothing.
- State rules of the Core (`src/OpenSpaceIA.Core/States`): the state of the moment of the chief and of an agent type (working, waiting for you, idle), "overloaded" and "under-used", computed from the periods of an agent with "now" and the settings (`StateSettings`, with their defaults) as parameters, never the clock.
- First test projects, with the shared test settings (`test/Directory.Build.props`: xUnit v3 on Microsoft.Testing.Platform, Shouldly, code coverage and TRX reports): `test/OpenSpaceIA.Core.Tests` for the model and the rules, written first, with the limit cases at every threshold.
- Architecture rules (`test/OpenSpaceIA.Architecture.Tests`): the Core depends on nothing but the .NET base library. Its project file has no project, package, assembly or framework reference, and every type its code uses (method bodies, generic arguments and attributes included, read by ArchUnitNET) lives in `System.*` or in the Core; a control proves that the external types are really seen.
- Roslynator analyzers for every project, so that the build enforces the Roslynator rules of `.editorconfig`.
- Roadmap: the logging convention is decided with the first code that writes logs.

### Changed

- Roadmap: the Doxygen configuration moves from 0.15.0 to 0.3.0, so that the documentation of the first code can be generated.
- CI: the test step always runs; the temporary skip used while there was no test project is removed.

## [0.2.0] - 2026-10-30

Spike on the files of Claude Code: the risky unknowns are proven on real files, before any product code; no code yet.

### Added

- Spike (`doc/analyse/spike-claude-files.md`): the structure of `~/.claude` that the reader will use (live sessions, transcripts, sub-agent runs, agent definitions, task lists), the files it must never read, the retention of the transcripts (30 days by default), and the sizes that make incremental reading necessary.
- Spike (`doc/analyse/spike-waiting-signal.md`): Claude Code sets the `status` of its session file to `waiting` while a question or a permission prompt waits for the user, and the tool call left open in the transcript tells which; both verified by live measurements. The transcript also records who decided each permission.
- Decisions of the spike (`doc/analyse/claude-model-decisions.md`): how the core and the reader turn the files of Claude Code into the states of the office (working, waiting for you with the reason, idle, closed session, sub-agent runs, overloaded), the history computed from the transcripts, the real model, the task list, the team file of our own, and the context of each session.

### Changed

- Roadmap: network tests use a range of ports reserved for the tests (defined in 1.1.0), so that test runs can work in parallel, instead of port 0; a new principle says that the IP address and the port of every application can always be changed in its configuration.
- Roadmap: a new version 0.12.0 shows the usage limits of the plan (5-hour window and weekly limit, as a percentage, with their reset times) and the context of each session, after a spike on where to read the limits; the write versions move to 0.13.0 and 0.14.0, packaging to 0.15.0.

## [0.1.0] - 2026-10-30

Repository bootstrap and project framing: purpose, architecture, stack, design and roadmap are defined; no code yet.

### Added

- Bilingual README (English and French): purpose (see whether the agents are overloaded or under-used), how the collector, the hub and the desktop application work together, planned requirements and repository layout.
- PolyForm Strict 1.0.0 license.
- Contribution guide describing the branching model, commit and pull request conventions, versioning and releases.
- Repository layout: `src/`, `test/`, `tools/` and `doc/` (analysis, assets, developer, user and administrator documentation), each with a README that states its purpose.
- Design mock-ups of the desktop application, in the style of a SEGA Mega Drive game: building in section, floor plates (chief office, code floor, lounge), street of machines, projects, teams and agents, workload history (`doc/assets/mockups`).
- Roadmap (`doc/analyse/roadmap.md`): scope of v1.0.0 (the complete desktop application of one machine, read and write) and of the releases after it (network, then web), versions from 0.1.0 to 1.6.0 with their pull requests, test strategy, risks and decisions.
- Changelog fragments: each pull request describes its changes in `changelog/unreleased/<branch>.md` (see the README of the folder); `tools/changelog/Merge-ChangelogFragments.ps1` moves them into `CHANGELOG.md` for the release.
  Taken from BoardGamesPlatform, with fixes so that nothing is lost or repeated silently: the summary under `[Unreleased]` and the continuation lines of existing entries are kept; an unknown heading or text in `[Unreleased]` stops the script; no line is repeated when `[Unreleased]` is the first or the last section; a fragment that cannot be deleted is reported, so that it is never merged twice.
- Continuous integration (GitHub Actions, Windows): on every pull request to `main`, `dev` or `release/*`, and after every merge into `main` or `dev`, it checks that the solution matches the projects on disk, refuses a pull request that edits `CHANGELOG.md` (except `docs/release-*`), restores, builds with warnings as errors, checks the code style with `dotnet format`, and runs the tests with TRX and coverage reports once a test project exists.
- Build settings: `global.json` (.NET 10 SDK, Microsoft.Testing.Platform), `Directory.Build.props` (warnings as errors, mandatory XML documentation, deterministic builds), `Directory.Packages.props` (central package management, no package yet).
- Code style: `.editorconfig`, enforced at build time, naming rules included.
- Solution `OpenSpaceIA.slnx`, with the repository files in solution folders; no project yet.
- Branch protection: the `Build and test` check of the CI is now required on `main`, `dev` and `release/*`; the two GitHub rulesets are kept in `.github/rulesets/`, and the contribution guide says how to create them again.

### Fixed

- CI: the changelog check now applies only to pull requests into a release branch, so that the pull requests that move a version up into `dev` and `main`, which carry the finalized changelog, are no longer refused.

[Unreleased]: https://github.com/micdef/OpenspaceIA/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/micdef/OpenspaceIA/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/micdef/OpenspaceIA/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/micdef/OpenspaceIA/releases/tag/v0.1.0
