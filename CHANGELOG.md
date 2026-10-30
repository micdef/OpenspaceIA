# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/micdef/OpenspaceIA/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/micdef/OpenspaceIA/releases/tag/v0.1.0
