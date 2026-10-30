# Tools

Reusable scripts for development and operation: PowerShell on Windows, bash on Linux.

| Script | Purpose |
|--------|---------|
| [changelog/Merge-ChangelogFragments.ps1](changelog/Merge-ChangelogFragments.ps1) | Moves the changelog fragments into `CHANGELOG.md`, for the `docs/release-X.Y.Z` pull request (see [changelog/unreleased](../changelog/unreleased/README.md)) |
| [doxygen/Convert-RecordsForDoxygen.ps1](doxygen/Convert-RecordsForDoxygen.ps1) | Input filter of Doxygen (set in the `Doxyfile`): presents the C# records as classes and structs, which Doxygen 1.18 does not otherwise document |
