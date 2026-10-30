# Roadmap

This document plans every version, from the repository bootstrap (`0.1.0`) to the first installable release (`1.0.0`), and the releases that follow it.
It records the scope, the order of the work, what each version must demonstrate, and the decisions taken so far.

Prepared by Mon Mothma (`planner`), reviewed by Claude, validated by Michaël Defraene on 2026-10-30.

## Scope

### In v1.0.0: the complete desktop application, read and write

- The animated office of **one machine** (the machine where the application runs): the street, the building seen in section, the floor plates (chief office, architecture and planning, code, review and tests, lounge).
- The states of the agents: **working** (at the desk), **overloaded** (chains several projects without a break: sweat and paper stacks), **waiting for you** (at the coffee machine, calls you with a "!" bubble), **idle** (relaxes: coffee, billiards, arcade, board games). Only idle counts as under-use.
- The Projects screen (in progress, waiting, done, interrupted) and the History screen (workload over 7 days).
- The discreet mode.
- **Writing, on this machine only**: create an agent, create a team (one per project if needed), assign agents and teams to a project; a backup before each change and a confirmation.
- Windows and Linux, installable from a package.
- User, administrator and developer documentation.

### After v1.0.0

| Item | Planned for |
|------|-------------|
| Network: threat model, contract, ports, authentication | 1.1.0 |
| Hub (SignalR, read only, in the home lab) | 1.2.0 |
| Collector on each machine | 1.3.0 |
| Desktop through the hub, local fallback, street with several machines | 1.4.0 |
| Start, stop and restart the collector from the desktop (elevation prompt) | 1.5.0 |
| Web application, read only | 1.6.0 |

## Principles

- **One theme per version**, small, and released fast. Each version ends with a demonstration, written below as its *done criteria*, and goes the full way: release → dev → main, annotated tag, GitHub pre-release (release with packages from 1.0.0).
- **One increment = one pull request**, on one scope. **Two pull requests open at the same time never touch the same file**; the files touched by many pull requests (`OpenSpaceIA.slnx`, `Directory.Packages.props`, `.github/workflows/ci.yml`, `README.md`) are changed by one pull request at a time.
- **Riskiest work first.** The format of `~/.claude` and the "waiting for you" signal are proven on real files (0.2.0) before any product code.
- **A tool, an abstraction or a package is added only by the version that needs it.** An architecture rule is added with the project it protects.
- **Tests from the start.** The core rules and the reader are written tests first (Kenobi).
- **Threat analysis before the code it covers**: writing agents (0.12.0), the network (1.1.0), the elevation prompt (1.5.0), the web (1.6.0).
- **Security-sensitive work is marked [SEC].** It is written by the main developer, never by a coder agent, and reviewed by the agents and by the project lead.
- **`~/.claude` is sensitive and untrusted.** The application reads it and never writes to it, except agent files created on purpose from 0.12.0; it sends nothing to a third party; the text of the transcripts is plain text, never rendered as HTML, never run.

## Architecture summary

A modular monolith, with a light hexagonal architecture (ports and adapters) inside the modules: a port only where something varies. Read ports and write ports are separate.

| Module | Role | From |
|--------|------|------|
| Core | Model (machine, project, session, agent, task) and state rules; knows neither the user interface nor the file format | 0.3.0 |
| Reader | Adapter that reads `~/.claude`; the only module that changes when the format of Claude Code changes | 0.4.0 |
| Desktop | Avalonia application, Windows and Linux; the only part that writes, on its own machine only | 0.5.0 |
| Writer | Adapter that creates agents and teams and assigns them, with backup and confirmation | 0.12.0 |
| Hub | SignalR server in the home lab (Linux service or IIS); gathers the machines; exposes reading only | 1.2.0 |
| Collector | Worker service on each machine (Windows service under the user's account, systemd user service on Linux); stops when the hub is not reachable | 1.3.0 |
| Web | Read only, through the hub | 1.6.0 |

## Versions

### Block 1 — Foundations and technical risks

#### 0.1.0 — Repository bootstrap

- **Content**: the bilingual README, the licence, the contribution guide and the changelog (already written); `global.json` (SDK 10, Microsoft.Testing.Platform), an empty `OpenSpaceIA.slnx`, `Directory.Build.props` (warnings as errors, XML documentation), an empty `Directory.Packages.props`, `.editorconfig`, `.gitattributes`; the changelog fragment tools; the CI workflow (restore, build, `dotnet format --verify-no-changes`, tests, `test-results` artifact; the solution guard; a pull request that edits `CHANGELOG.md` is refused, except `docs/release-X.Y.Z`); the branch protection rules (set by the project lead).
- **Done**: the CI is green on an empty pull request; a direct push to `main` or `dev` is refused; the tag `v0.1.0` is set, the same day.
- **Tests**: no test project yet; the guard "a test project runs at least one test" acts from the first test project (0.3.0).
- **Branches**: `chore/solution-layout` and `chore/changelog-tools` (in parallel), then `chore/ci-workflow`, then `docs/branch-protection`.

#### 0.2.0 — Spike: the files of Claude Code and the waiting signal

- **Content**: no product code. An inventory of `~/.claude` (sessions, sub-agents, agent definitions, projects, size of the folders) with **invented** examples (never a real transcript in Git); an experiment that triggers a permission request, a question to the user and a final message, and records what changes in the files; the decisions that follow: which states can be detected, the **fallback if the waiting signal cannot be detected**, and how a team and an assignment to a project are represented on disk.
- **Done**: a page says, with evidence, "waiting is detected this way" or "it cannot be detected, we take fallback X".
- **Fallback options** (not verified, the spike decides): (a) "waiting, probably", after a time threshold, labelled as a heuristic; (b) three states plus "unknown". Hooks in the Claude Code settings are not an option: they would write to `~/.claude`.
- **Branches**: `docs/claude-files-inventory` and `docs/waiting-signal-experiment` (in parallel), then `docs/claude-model-decisions`. A throwaway observation script in `tools/spikes/` only if needed, removed at the end of the spike.

#### 0.3.0 — Core: model and state rules

- **Content**: the written definition of the four states, of "overloaded" (threshold and time window, configurable) and of "under-used"; the Core project (machine, project, session, agent, task) with its read port; the state rules with an injectable clock; the first architecture rules (Core depends on nothing, with a positive control).
- **Done**: named scenarios (at the desk, chaining without a break, waiting, resting) give the right state; the CI runs its first tests, with coverage and TRX reports.
- **Tests**: written first (Kenobi); limit cases at the thresholds; fake clock; an agent is never working and idle at the same time.
- **Branches**, in order: `docs/state-definitions`, `feature/core-model`, then `feature/state-rules` and `test/architecture-rules` (in parallel only if neither adds a package).

#### 0.4.0 — The reader of `~/.claude`

- **Content**: the Reader project with invented fixtures; a tolerant reading: an unknown or unreadable line is skipped and logged, never an exception that goes up; agent definitions; sessions and sub-agents turned into observations for the rules; the waiting signal (or its fallback) from 0.2.0; files opened with full sharing so that Claude Code is never disturbed.
- **Done**: a local test, skipped in the CI, shows the real state of your agents; a missing, broken or locked file never stops the reader.
- **Tests**: written first (Kenobi), reviewed by Boba Fett; truncated files, encodings, a very large file (stream reading); the reader creates and writes nothing (test on a read-only folder); it only returns what the core needs.
- **Branches**, in order: `feature/reader-project`, then `feature/reader-agents` and `feature/reader-activity` (in parallel), then `feature/reader-waiting-signal`, then `test/reader-local-smoke`.

### Block 2 — The desktop application, read

#### 0.5.0 — Desktop shell, local mode

- **Content**: the desktop conventions (light MVVM written by hand, polling first, "every text from a transcript is plain text"); a Linux job in the CI; the Avalonia project with the **build telemetry of Avalonia turned off** (`AVALONIA_TELEMETRY_OPTOUT=1`, a pitfall verified on the first repository); the window, the navigation (Street, Building, Projects, Teams, History), the neon theme, embedded fonts; the agent list with a state badge, refreshed by polling.
- **Done**: the application starts on Windows and builds in the Linux job; the list shows the real state of your agents.
- **Tests**: the view model of the list; no control that renders HTML or Markdown from transcript text. Avalonia headless tests with xUnit v3: not verified, checked in this version.
- **Branches**: `docs/desktop-conventions` and `chore/ci-linux` (in parallel), then `feature/desktop-shell`, then `feature/desktop-agent-list`.

#### 0.6.0 — Scene engine and building view

- **Content**: the rendering technique (Avalonia drawing or Skia), the format of the sprite sheets, the animation loop; **the sprites are drawn for the project**, like the mock-ups, with no third-party asset pack (no licence conflict with PolyForm Strict); the sprite loader and a pixel-perfect rendering (no smoothing); the building in section, each role placed on its floor.
- **Done**: the building of the mock-up shows your agents on the right floor, still; the frame rate on your PC is measured (target not verified).
- **Tests**: role → floor mapping; animation clock with a fake clock; no "pixel exact" image test (too fragile).
- **Branches**, in order: `docs/scene-technique`, `feature/scene-engine`, `feature/building-view`.

#### 0.7.0 — Floor plates

- **Content**: from the building to one floor plate, a shared frame and side panel; the chief office with the task list of the main session; the work floors (code, review and tests, architecture and planning) with their task cards; the lounge.
- **Done**: the five floor plates open and show their agents and cards.
- **Tests**: the view model of each plate; a card title is plain text, cut to a maximum length, never interpreted.
- **Branches**: `feature/floor-plate-base`, then `feature/floor-chief`, `feature/floor-work`, `feature/floor-lounge` (in parallel, each in its own files).

#### 0.8.0 — Animated states

- **Content**: working and overloaded (sweat, paper stacks); idle (coffee, billiards, arcade, board games); waiting for you (coffee machine, "!" bubble), only if the spike succeeded, otherwise its fallback.
- **Done**: each state has its own picture, and a change of state shows without restarting the application.
- **Tests**: state → animation table; an unknown state falls back to a visible "unknown", never a crash.
- **Branches**: `feature/state-animations-working` and `feature/state-animations-lounge` (in parallel), then `feature/state-animations-waiting`.

#### 0.9.0 — Projects screen

- **Content**: the rules of the project states (in progress, waiting, done, interrupted) in the core; the screen.
- **Done**: your real projects are listed with the right state.
- **Tests**: written first; state transitions; a project without activity for N days.
- **Branches**, in order: `docs/project-states`, `feature/core-project-states`, `feature/projects-screen`.

#### 0.10.0 — History screen

- **Content**: the workload over 7 days (working, working nonstop, waiting for you, idle), **computed again from the files**, with no database of our own; the screen.
- **Done**: you see your week and can tell which agent was overloaded or under-used.
- **Tests**: aggregation (Kenobi); a period without data; a change of daylight saving time.
- **Branches**: `feature/core-workload-history` and `feature/reader-timeline` (in parallel), then `feature/history-screen`.

#### 0.11.0 — Street and discreet mode

- **Content**: the street with one building (this machine); the discreet mode: it hides every text that comes from the transcripts and keeps the agent names and states.
- **Done**: the street shows; the discreet mode hides what was decided.
- **Tests**: in discreet mode, no transcript text is in the view models.
- **Branches**: `feature/street-view` and `feature/discreet-mode` (in parallel).

### Block 3 — The desktop application, write

#### 0.12.0 — Write: create an agent [SEC]

- **Content**: the threat model of writing (allowed paths only, name validation, backup before each change, confirmation, this machine only); the Writer project with its backup; creating an agent from the "New agent" panel; an architecture rule: the core and the reader never reference the writer.
- **Done**: you create an agent from the Teams screen; the file is written only where planned, after a backup and your confirmation; Claude Code sees the new agent.
- **Tests**: forbidden paths and names are refused; the backup exists before the write; a failed write leaves the previous state untouched.
- **Branches**, in order: `docs/threat-model-writing`, `feature/writer-backup`, `feature/writer-create-agent`, `feature/teams-screen-new-agent`.

#### 0.13.0 — Write: teams and assignment [SEC]

- **Content**: the team model (stored in a file of our own, outside `~/.claude`, unless the spike shows a better place); the assignment of agents and teams to a project; the complete Teams screen.
- **Done**: you create a team, add agents, assign it to a project, and the office shows it.
- **Tests**: as in 0.12.0, for every write; removing a team asks for a confirmation and keeps a backup.
- **Branches**, in order: `feature/team-model`, `feature/team-assignment`, `feature/teams-screen-teams`.

### Block 4 — Towards v1.0.0

#### 0.14.0 — Packaging and hardening

- **Content**: self-contained publication for `win-x64` and `linux-x64`; the release workflow on a tag; the Doxygen configuration (`doc/dev`, HTML not versioned); the user and administrator documentation; a security review by Ackbar and Cassian, on request of the project lead (read-only access to `~/.claude`, writing limits, no network output, dependencies).
- **Done**: the package installs on a clean Windows machine and a clean Linux machine.
- **Branches**: `chore/publish-scripts`, `docs/user-and-admin-docs` and `chore/doxygen` (in parallel), then `chore/release-workflow`.

#### 1.0.0-beta.N and 1.0.0-rc.N — Hardening

- **Done**: daily use on your PC for one to two weeks without a blocking bug.

#### 1.0.0 — Release

- **Done**: clean installation and complete verification run; final changelog and tag; packages attached to the GitHub release.

### Block 5 — After v1.0.0: the network

Each version is a release with packages. A fix between two versions goes through a `hotfix/…` branch.

#### 1.1.0 — Network: threat model and contract

- **Content**: documents only: the threat model of the network (collector ↔ hub ↔ clients), the wire contract and the ports, the choice of the authentication (one identity per collector, so that one machine can be removed without touching the others), TLS.
- **Branches**: `docs/threat-model-network`, `docs/wire-contract-and-ports`, `docs/authentication-choice`.

#### 1.2.0 — Hub [SEC]

- **Content**: the contracts; the authenticated ingestion from the collectors; the read API for the clients, with **no write method** (checked by an architecture test); hosting as a Linux service or on IIS; optional OpenTelemetry export, only when an address is configured.
- **Branches**, in order: `feature/contracts`, `feature/hub-ingest-auth`, `feature/hub-read-api`.

#### 1.3.0 — Collector [SEC]

- **Content**: the worker service, which reads `~/.claude` through the reader and sends to the hub, and **stops when the hub is not reachable**; installation as a Windows service under your account and as a systemd user service on Linux.
- **Branches**: `feature/collector-service`, then `chore/collector-install-windows` and `chore/collector-install-linux` (in parallel).

#### 1.4.0 — Desktop through the hub, several machines

- **Content**: the desktop reads through the hub, with an online / local badge; it falls back to reading `~/.claude` directly when the hub is not reachable; the street shows one building per machine.
- **Branches**, in order: `feature/desktop-hub-source`, `feature/desktop-source-fallback`, `feature/street-multi-machine`.

#### 1.5.0 — Collector control [SEC]

- **Content**: the threat model of the elevation; start, stop and restart the collector from the desktop through the system prompt (UAC on Windows, polkit on Linux); the application itself never runs with administrator rights and never sees a password.
- **Branches**, in order: `docs/threat-model-elevation`, `feature/collector-control`.

### Block 6 — After v1.0.0: the web

#### 1.6.0 — Web, read only

- **Content**: the threat model of the web; the read-only web application through the hub; an architecture rule: the web never references the writer.
- **Branches**, in order: `docs/web-threat-model`, `feature/web-readonly`.

### Slack

If time runs short, 0.9.0 (Projects) and 0.10.0 (History) can be merged into one version, and 0.11.0 (Street) can move after 0.13.0, without affecting the rest.

## Test strategy

| Level | Covers | From |
|-------|--------|------|
| Architecture | Core depends on nothing; the core and the reader never reference the writer; the hub exposes no write method; the web never references the writer | 0.3.0 |
| Core | State rules, project states, workload aggregation; pure code with an injectable clock | 0.3.0 |
| Reader | Invented fixtures, broken and large files, read-only folder, full file sharing | 0.4.0 |
| Desktop | View models; plain text only; discreet mode | 0.5.0 |
| Writer | Allowed paths, backup before write, failed write leaves the previous state | 0.12.0 |
| Network | Loopback sockets on port 0, authentication, reconnection; several runs | 1.2.0 |

- Test projects live in `test/` and all run in the CI; tests never use a real transcript.
- Tests wait for events, never for fixed delays; the only accepted delay is one that proves an event does **not** happen, and it is documented on the test.

## Risks

| Risk | Addressed in |
|------|--------------|
| The "waiting for you" signal cannot be detected in the files | 0.2.0: experiment on real files, and a fallback decided before any code |
| Claude Code changes the format of `~/.claude` | 0.4.0: one reader adapter, tolerant reading, a visible "unknown" instead of wrong data |
| Claude Code deletes old files, so the history is incomplete (not verified) | 0.2.0 checks it; a small storage of our own only if needed |
| Writing breaks an agent or a project | 0.12.0: threat model, allowed paths, backup, confirmation, this machine only |
| A secret or a hostile text from a transcript reaches the screen | Plain text only; discreet mode in 0.11.0 |
| The Avalonia build sends telemetry | 0.5.0: `AVALONIA_TELEMETRY_OPTOUT=1` in the CI and on the workstations |
| Licence of the pixel art | 0.6.0: sprites drawn for the project, no third-party pack |
| Scope creep | Done criteria per version; slack above |

## Decisions taken

| Topic | Decision |
|-------|----------|
| Purpose | See who works on what, to judge whether the agents are overloaded or under-used |
| Agent states | Working, overloaded, waiting for you, idle; only idle counts as under-use; the coffee machine is the break, an agent who waits for you calls you from there |
| Architecture | Modular monolith; light hexagonal architecture in the modules; separate read and write ports |
| Who writes | The desktop only, on the machine where it runs; the hub and the web are read only |
| Desktop | Avalonia, Windows and Linux |
| Hub and collector | SignalR hub in the home lab (Linux service or IIS); one collector per machine, under the user's account; it stops when the hub is not reachable; the desktop restarts it through the system elevation prompt |
| Stack | .NET 10 LTS; quality baseline of BoardGamesPlatform (warnings as errors, central package management, Roslynator, xUnit v3 on Microsoft.Testing.Platform, Shouldly, ArchUnitNET, coverage and TRX) |
| Design | Mega Drive pixel art scene in a neon frame; validated mock-ups in `doc/assets/mockups` |
| Principle | Least privilege everywhere |
| Licence | PolyForm Strict 1.0.0 |
| Scope of v1.0.0 | The complete desktop application of one machine, read and write; the network and the web come after |

## Choices left for later

| Choice | When |
|--------|------|
| Threshold and time window of "overloaded" | 0.3.0, calibrated on a real week |
| History computed from the files, or stored | 0.10.0, after the spike |
| Where a team lives | 0.13.0, after the spike (default: a file of our own, outside `~/.claude`) |
| Language of the interface (English only, texts in a resource file) | 0.5.0 |
| Polling or file watching | 0.5.0, polling first, measured before any change |
| A MVVM package, only if the hand-written view models become painful | 0.5.0 |
