### Added

- Changelog fragments: each pull request describes its changes in `changelog/unreleased/<branch>.md` (see the README of the folder); `tools/changelog/Merge-ChangelogFragments.ps1` moves them into `CHANGELOG.md` for the release.
  Taken from BoardGamesPlatform, with fixes so that nothing is lost or repeated silently: the summary under `[Unreleased]` and the continuation lines of existing entries are kept; an unknown heading or text in `[Unreleased]` stops the script; no line is repeated when `[Unreleased]` is the first or the last section; a fragment that cannot be deleted is reported, so that it is never merged twice.
