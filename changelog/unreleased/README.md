# Changelog fragments

Every pull request describes its changes in **its own file** of this folder, never in `CHANGELOG.md`.
Two pull requests open at the same time therefore never touch the same lines, and never conflict on the changelog.

## Writing a fragment

- One file per pull request, named after its branch with `/` replaced by `-`: `feature/street-view` → `feature-street-view.md`.
- Only `### <Section>` headings of [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) (`Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`), each followed by `- ` entries; a long entry may continue on lines indented by two spaces.

```markdown
### Added

- Street: one building per machine.

### Fixed

- A closed session no longer stays in the office.
```

## Releasing

The `docs/release-X.Y.Z` pull request runs `pwsh tools/changelog/Merge-ChangelogFragments.ps1`, which moves every fragment into the `[Unreleased]` section of `CHANGELOG.md` and deletes it, then finalizes the version as described in [CONTRIBUTING.md](../../CONTRIBUTING.md). The summary written under `[Unreleased]` is kept.
The CI refuses any other pull request into a release branch that edits `CHANGELOG.md` (`.github/scripts/Test-ChangelogUntouched.ps1`); the pull requests that move a version up into `dev` and `main` carry the finalized changelog.
