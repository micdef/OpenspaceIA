# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.0] - 2026-10-31

The reader of `~/.claude`: a read-only adapter that turns the files of Claude Code into a snapshot of the office (agent types, open sessions with the reason of a wait, sub-agent runs, periods of the chief), tolerant of missing, broken or locked files, with "under-used" calibrated on a real week; no user interface yet.

### Added

- Reader: the agent definitions of the user (`agents/*.md` of Claude Code) become the agent types of the core: name, description and model, read from the front matter by a small parser of our own (the simple forms of YAML only, no package of a third party). The reading is tolerant: a definition that cannot be read or parsed, a value in a form that is not read, and a second definition of the same name are each reported as a problem with their file, and never stop the reading of the others.
- Reader: the periods of work and of waiting of the chief, read in the transcripts of its sessions written in the window, only where they grew. A turn goes from a message of the user to a final answer, or to an interruption by the user; a message typed while Claude works continues the turn; a local command or a summary of a compaction does not start a turn; a silence of more than 30 minutes without a tool call in progress ends the turn at its last record. A period found in two transcripts (a resumed session) is kept once. A wait is the time from a tool call to its result when the user decided. The periods of work are the turns minus their waits. A record dated more than five minutes after now is not read; the transcripts of the sub-agents and the memory folders are never read as a chief.
- Reader project (`src/OpenSpaceIA.Reader`), the adapter that reads the files of Claude Code, read only, and depends on the Core only. Its base: the folder of Claude Code (`~/.claude` by default, any other folder can be given), an allow-list of the only files it may open (`sessions/*.json`, the `.jsonl` transcripts of `projects/` outside `memory/`, `agents/*.md`; NTFS streams, junctions and symbolic links below the folder are refused, so that no trick of the file system reopens a forbidden folder), and a safe reading of a small text file: opened read-only with full sharing so that Claude Code is never disturbed, limited in size, and a missing, locked, refused or forbidden file gives an explicit failure instead of an exception.
- Tests of the Reader (`test/OpenSpaceIA.Reader.Tests`), written first, on invented files in a temporary folder; architecture rules of the Reader (it depends on the Core only).
- Reader: the transcripts are read line by line, with the same safety as the other files (allow-list, links refused, full sharing), so that a transcript of hundreds of megabytes is never held in memory; a line longer than the limit is skipped and counted.
- Reader: the runs of the sub-agents written in the window (`projects/<project>/<session>/subagents/agent-<id>.jsonl`), each with its agent type (from its description `agent-<id>.meta.json`), its session, its project, its first and last records, its end and its real model. A run without description is left out and reported; each fact of a line is read on its own, and the lines that cannot be read are reported once per file; a time must be a full ISO date with its offset, and a record dated more than five minutes after now is not read; the memory folders are never walked. A run whose files have not changed is taken from memory instead of being read again; nothing is written on the disk.
- Core: the periods of work of an agent type come from its runs (`RunActivity`), and an agent seen only in runs (a built-in agent, an agent of a plugin) gets a desk.
- Reader: the open sessions of Claude Code (`sessions/*.json`). Each session file gives its process, its project, its start and its status; the key files next to it are never opened. A session whose process is gone, or whose process ID now belongs to a process started at another time, is closed and left out; one whose process cannot be checked is kept and reported. The process is checked on Windows (start time of the process) and on Linux (`/proc/<pid>/stat`), as measured on both systems.
- Reader: `ClaudeActivityReader`, the adapter of the read port of the core. Each snapshot gathers the agent definitions, the open sessions with the reason of a wait, the sub-agent runs and the periods of the chief over the window of history, read outside the thread of the caller; a session whose status is not written yet is left out until the next snapshot. The problems of every part come with the snapshot.
- Core: a problem met while the activity was read (`ReadingProblem`), and the snapshot now holds the periods of the chief and the problems of the reading.
- Reader: a transcript can be read only where it grew: the complete lines after a position are read, with the same safety as the other files, and a line that Claude Code is still writing is left for the next reading.
- Reader: the reason why a chief waits for the user. For each session whose status is waiting, its transcript is read from where the previous reading stopped, and the tool call left open gives the reason: a question for `AskUserQuestion` and for the approval of a plan (`ExitPlanMode`), a permission for any other tool, and a question when no call is open yet (the status of a question can come before its record). A waiting session always gets a reason; a missing or unreadable transcript is reported.
- Tests: a local smoke test that reads the real folder of Claude Code of the user and writes the state of the chief and of each agent type, with the times and the problems. It is explicit: a normal run of the tests and the CI never run it, so the files of the user are never read without asking; `test/README.md` says how to run it.

### Changed

- Core: "under-used" is calibrated on a real week. An agent is now under-used when it was idle at least 95 % of the active time of the chief (its work and its waiting) over 7 days, instead of 90 % of the whole 7 days, which flagged every agent; it does not apply to the chief, and no agent is under-used when the chief was never active. The thresholds of "overloaded" are confirmed (2 hours, 2 projects, a break under 10 minutes). `doc/analyse/state-definitions.md` gives the measures behind the choice.
- Reader: the agent definitions in the subfolders of `agents/` are read too, as Claude Code reads them; the allow-list accepts `agents/.../<name>.md`. A subfolder that is a junction or a symbolic link is not followed, and it is reported.
- Reader: the following of a transcript only where it grew, and the reading of the time of a record, are shared by the readers of the reason of a wait, of the runs and of the periods of the chief.
- State definitions: how the turns and the waits of the chief are read in the transcripts.
- Reader: the allow-list also accepts the descriptions of the sub-agent runs, at their exact place (`projects/<project>/<session>/subagents/agent-<id>.meta.json`), outside the memory folders.
- Decision 4 of the model of Claude Code: a run without its description is left out and reported.
- Roadmap: a line that cannot be read is reported in the result of the reader, and the host writes the logs; `feature/reader-activity` is split into `feature/reader-sessions` and `feature/reader-runs`.
- Spike on the files of Claude Code: what `procStart` holds on Windows and on Linux, the status written about one second after the file, and the file deleted when the session ends.
- Reader: a line of a transcript has its own size limit, 16 MiB by default, apart from the limit of 1 MiB of a small file, so that the result of reading a document (up to 1.92 MB measured) is never skipped; a skipped result would leave its tool call open for ever.
- Spike of the waiting signal: the time between a tool call and its result, measured by source of the decision on a week of transcripts, and the definition of a period of waiting of the chief.
- Roadmap: `feature/reader-chief-periods` comes after `feature/reader-waiting-signal`.

### Fixed

- Spike on the files of Claude Code: an agent is identified by the `name` of its front matter, not by its file name, and Claude Code also reads the subfolders of `agents/` (Claude Code documentation).
- Tests of the Reader: the test of a description rewritten with the same size failed on the CI (2 times out of 2 on the push into `dev`, 2026-10-31), since two writes a few milliseconds apart got the same last write and the cache of the runs knows a file by its size and its last write only. The test now sets the new last write itself, and a new test pins the known limit of the cache: a rewrite that keeps the size and the last write is not seen until the next write (`CachedRun`). Fixed by an exception to the release process, decided by Michaël Defraene: `release/0.4.0` was already merged into `dev` and deleted, so the fix went from `fix/run-cache-test`, made from `dev`, into `dev`; the process of `CONTRIBUTING.md` is unchanged.

## [0.3.0] - 2026-10-30

The Core: the immutable model of the office and the rules that judge the workload of the agents, written first as tests, with no dependency; no user interface yet.

### Added

- State definitions (`doc/analyse/state-definitions.md`): an agent is an agent type, and the chief is one of them; the state at a given moment (working, waiting for you with its reason, idle); overloaded (over the last 2 hours, at least 2 projects and no break of 10 minutes or more); under-used (idle at least 90 % of the last 7 days); every value is a default that the user can change. Then the periods given to the rules, the merge of the periods that overlap, and how the thresholds are read (the start and the end of the window count as breaks; a period counts only for a part of more than zero inside the window).
- Coding conventions (`doc/dev/coding-conventions.md`): file headers, usings, regions, documentation of every member, step comments and code rules for C#; and the tests, named in PascalCase by the rule they prove, with "now" as a parameter and invented data only.
- Doxygen configuration (`Doxyfile` at the root): the HTML documentation of `src/`, with the README as main page and the contribution guide, the changelog, the analysis documents and the coding conventions; private members and internal types are published too, since they are documented. Doxygen 1.18 does not know the C# `record` keyword, so the C# files are read through a filter (`tools/doxygen/Convert-RecordsForDoxygen.ps1`) that presents the records as classes and structs, without changing the sources. The output in `doc/dev/html/` is not versioned.
- Core project (`src/OpenSpaceIA.Core`): the immutable model of the office (machine, project, session of the chief, agent type, run of a sub-agent, task of a task list, period of work or of waiting, snapshot of the activity) and its read port. Every value is checked when it is built (times in UTC, a reason for every waiting session, lists copied); the path of a project loses its trailing separators, so that one folder is always one project. The project depends on nothing.
- State rules of the Core (`src/OpenSpaceIA.Core/States`): the state of the moment of the chief and of an agent type (working, waiting for you, idle), "overloaded" and "under-used", computed from the periods of an agent with "now" and the settings (`StateSettings`, with their defaults) as parameters, never the clock.
- First test projects, with the shared test settings (`test/Directory.Build.props`: xUnit v3 on Microsoft.Testing.Platform, Shouldly, code coverage and TRX reports): `test/OpenSpaceIA.Core.Tests` for the model and the rules, written first, with the limit cases at every threshold.
- Architecture rules (`test/OpenSpaceIA.Architecture.Tests`): the Core depends on nothing but the .NET base library. Its project file has no project, package, assembly or framework reference, and every type its code uses (method bodies, generic arguments and attributes included, read by ArchUnitNET) lives in `System.*` or in the Core; a control proves that the external types are really seen.
- Roslynator analyzers for every project, so that the build enforces the Roslynator rules of `.editorconfig`.
- Roadmap: the logging convention is decided with the first code that writes logs.

### Changed

- Roadmap: the Doxygen configuration moves from 0.15.0 to 0.3.0, so that the documentation of the first code can be generated.
- Roadmap: the thresholds of "overloaded" have their defaults since 0.3.0; their calibration on a real week moves to 0.4.0, since it needs the reader of the files.
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

[Unreleased]: https://github.com/micdef/OpenspaceIA/compare/v0.4.0...HEAD
[0.4.0]: https://github.com/micdef/OpenspaceIA/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/micdef/OpenspaceIA/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/micdef/OpenspaceIA/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/micdef/OpenspaceIA/releases/tag/v0.1.0
