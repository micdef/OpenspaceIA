### Added

- Continuous integration (GitHub Actions, Windows): on every pull request to `main`, `dev` or `release/*`, and after every merge into `main` or `dev`, it checks that the solution matches the projects on disk, refuses a pull request that edits `CHANGELOG.md` (except `docs/release-*`), restores, builds with warnings as errors, checks the code style with `dotnet format`, and runs the tests with TRX and coverage reports once a test project exists.
