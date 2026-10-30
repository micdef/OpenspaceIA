# ┌──────────────┬──────────────────────────────────────────────────────────────┐
# │ File         │ .github/scripts/Test-ChangelogUntouched.ps1                  │
# │ Purpose      │ Fails when a pull request into a release branch edits        │
# │              │ CHANGELOG.md, except the docs/release-X.Y.Z pull request:    │
# │              │ every other one writes a fragment in changelog/unreleased/.  │
# │ Rules        │ 1. Only pull requests into release/* are checked: a pull     │
# │              │    request into dev or main moves a version up, with its     │
# │              │    finalized changelog.                                      │
# │              │ 2. A branch named docs/release-* may edit CHANGELOG.md.      │
# │              │ 3. The changed files are those between the merge base of the │
# │              │    target branch and the pull request.                       │
# │ Usage        │ pwsh .github/scripts/Test-ChangelogUntouched.ps1             │
# │              │      -BaseRef <target branch> -HeadRef <pull request branch> │
# │ Exit code    │ 0 when the rule is kept, 1 otherwise (or when Git fails).    │
# └──────────────┴──────────────────────────────────────────────────────────────┘

[CmdletBinding()]
param(
    # Target branch of the pull request, for example release/0.1.0 (it must be fetched as origin/<BaseRef>)
    [Parameter(Mandatory)] [string] $BaseRef,

    # Branch of the pull request, for example chore/ci-workflow
    [Parameter(Mandatory)] [string] $HeadRef,

    # Root of the repository; by default, two levels above this script
    [string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

# Rule 1: the fragments are written by the work merged into a release branch; moving a version up into dev or main
# carries the changelog that the docs/release-X.Y.Z pull request finalized
if ($BaseRef -notlike 'release/*')
{
    Write-Host "This pull request targets $BaseRef, not a release branch: it moves a finalized version up, the rule does not apply."
    exit 0
}

# Rule 2: the release pull request is the only one that finalizes the changelog
if ($HeadRef -like 'docs/release-*')
{
    Write-Host "$HeadRef is a release pull request: it may edit CHANGELOG.md."
    exit 0
}

# Rule 3: files changed by the pull request only (three dots: from the merge base, not from the tip of the target).
# --no-renames: a renamed CHANGELOG.md is listed under its old name too, so that a rename cannot pass the check.
$changed = @(git -C $RepositoryRoot diff --no-renames --name-only "origin/$BaseRef...HEAD")
if ($LASTEXITCODE -ne 0)
{
    # Without the list of changed files, the rule cannot be checked: fail rather than pass blindly
    Write-Host "::error::git diff failed (exit code $LASTEXITCODE): is origin/$BaseRef fetched with its history?"
    exit 1
}

if ($changed -contains 'CHANGELOG.md')
{
    Write-Host '::error::This pull request edits CHANGELOG.md. Write its changes in changelog/unreleased/<branch>.md instead (see changelog/unreleased/README.md).'
    exit 1
}

Write-Host "CHANGELOG.md is untouched ($($changed.Count) file(s) changed)."
exit 0
