# ┌──────────────┬──────────────────────────────────────────────────────────────┐
# │ File         │ tools/changelog/Merge-ChangelogFragments.ps1                 │
# │ Purpose      │ Moves the changelog fragments of the merged pull requests    │
# │              │ into the [Unreleased] section of CHANGELOG.md, then deletes  │
# │              │ them; run by the docs/release-X.Y.Z pull request only.       │
# │ Rules        │ 1. A fragment is changelog/unreleased/<branch>.md, holding   │
# │              │    "### <Section>" headings followed by "- " entries.        │
# │              │ 2. Sections: Added, Changed, Deprecated, Removed, Fixed,     │
# │              │    Security (Keep a Changelog); any other text is refused.   │
# │              │ 3. Entries keep the order of the fragment files (by name).   │
# │              │ 4. The summary under [Unreleased] and the continuation lines │
# │              │    of existing entries are kept; any other text there stops  │
# │              │    the script instead of being lost.                         │
# │ Usage        │ pwsh tools/changelog/Merge-ChangelogFragments.ps1 [-WhatIf]  │
# │ Exit code    │ 0 on success, 1 when a fragment is invalid (nothing written).│
# └──────────────┴──────────────────────────────────────────────────────────────┘

[CmdletBinding(SupportsShouldProcess)]
param(
    # Root of the repository; by default, two levels above this script
    [string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

# Sections of Keep a Changelog, in their documented order
$sections = @('Added', 'Changed', 'Deprecated', 'Removed', 'Fixed', 'Security')

$changelogPath = Join-Path $RepositoryRoot 'CHANGELOG.md'
$fragmentFolder = Join-Path $RepositoryRoot 'changelog\unreleased'

# Step 1: read every fragment (the README of the folder is not one), refusing anything outside the expected shape
$fragments = @(Get-ChildItem -Path $fragmentFolder -Filter '*.md' -File | Where-Object Name -ne 'README.md' | Sort-Object Name)
if ($fragments.Count -eq 0)
{
    Write-Host 'No changelog fragment to merge.'
    exit 0
}

$newEntries = [ordered]@{}
foreach ($section in $sections) { $newEntries[$section] = [Collections.Generic.List[string]]::new() }
$errors = [Collections.Generic.List[string]]::new()

foreach ($fragment in $fragments)
{
    $current = $null
    $lineNumber = 0
    foreach ($line in Get-Content -Path $fragment.FullName)
    {
        $lineNumber++
        if ($line -match '^### (?<name>\w+)\s*$')
        {
            $current = $Matches['name']
            if ($current -notin $sections) { $errors.Add("$($fragment.Name):$lineNumber unknown section '$current'") }
        }
        elseif ($line -match '^- \S')
        {
            if ($null -eq $current) { $errors.Add("$($fragment.Name):$lineNumber entry before any '### <Section>' heading") }
            elseif ($current -in $sections) { $newEntries[$current].Add($line) }
        }
        elseif ($line -match '^  \S' -and $null -ne $current -and $current -in $sections -and $newEntries[$current].Count -gt 0)
        {
            # Continuation line of the previous entry (indented by two spaces)
            $last = $newEntries[$current].Count - 1
            $newEntries[$current][$last] = $newEntries[$current][$last] + "`n" + $line
        }
        elseif ($line.Trim().Length -gt 0)
        {
            $errors.Add("$($fragment.Name):$lineNumber unexpected text (only '### <Section>' headings and '- ' entries are allowed)")
        }
    }
}

if ($errors.Count -gt 0)
{
    $errors | ForEach-Object { Write-Host "::error::$_" }
    exit 1
}

# Step 2: read the [Unreleased] section of the changelog and its current entries
$lines = [IO.File]::ReadAllLines($changelogPath)
$start = [Array]::IndexOf($lines, '## [Unreleased]')
if ($start -lt 0) { Write-Host '::error::CHANGELOG.md has no "## [Unreleased]" heading'; exit 1 }
$end = $start + 1
while ($end -lt $lines.Length -and -not $lines[$end].StartsWith('## [')) { $end++ }

$existingEntries = [ordered]@{}
foreach ($section in $sections) { $existingEntries[$section] = [Collections.Generic.List[string]]::new() }
# Text written under "## [Unreleased]" before its first "### <Section>" heading (a summary of the version) is kept as it is
$preamble = [Collections.Generic.List[string]]::new()
$current = $null
for ($i = $start + 1; $i -lt $end; $i++)
{
    if ($lines[$i] -match '^###')
    {
        # Only the Keep a Changelog sections are rebuilt: any other heading (or a heading with extra words) would be lost
        if ($lines[$i] -notmatch '^### (?<name>\w+)\s*$' -or $Matches['name'] -notin $sections)
        {
            Write-Host "::error::CHANGELOG.md:$($i + 1) unknown section heading in the [Unreleased] section: '$($lines[$i])'"
            exit 1
        }
        $current = $Matches['name']
    }
    elseif ($null -eq $current) { $preamble.Add($lines[$i]) }
    elseif ($lines[$i] -match '^- \S' -and $existingEntries.Contains($current)) { $existingEntries[$current].Add($lines[$i]) }
    elseif ($lines[$i] -match '^  \S' -and $existingEntries.Contains($current) -and $existingEntries[$current].Count -gt 0)
    {
        # Continuation line of an existing entry (indented by two spaces): kept with its entry
        $last = $existingEntries[$current].Count - 1
        $existingEntries[$current][$last] = $existingEntries[$current][$last] + "`n" + $lines[$i]
    }
    elseif ($lines[$i].Trim().Length -gt 0)
    {
        # Anything else would be lost when the section is rebuilt: stop instead of dropping it silently
        Write-Host "::error::CHANGELOG.md:$($i + 1) unexpected text in the [Unreleased] section"
        exit 1
    }
}
# The summary, without its leading and trailing empty lines
while ($preamble.Count -gt 0 -and $preamble[0].Trim().Length -eq 0) { $preamble.RemoveAt(0) }
while ($preamble.Count -gt 0 -and $preamble[$preamble.Count - 1].Trim().Length -eq 0) { $preamble.RemoveAt($preamble.Count - 1) }

# Step 3: rebuild the [Unreleased] section: existing entries first, then the fragments, section by section
$section = [Collections.Generic.List[string]]::new()
$section.Add('## [Unreleased]')
if ($preamble.Count -gt 0)
{
    $section.Add('')
    foreach ($line in $preamble) { $section.Add($line) }
}
foreach ($name in $sections)
{
    $entries = @($existingEntries[$name]) + @($newEntries[$name])
    if ($entries.Count -eq 0) { continue }
    $section.Add('')
    $section.Add("### $name")
    $section.Add('')
    foreach ($entry in $entries) { $section.Add($entry) }
}
$section.Add('')

# The rest of the file (the released versions). When [Unreleased] is the last section there is none: a PowerShell range
# such as $end..($lines.Length - 1) would then count backwards and repeat the last line, so it is only built when needed.
$tail = if ($end -lt $lines.Length) { @($lines[$end..($lines.Length - 1)]) } else { @() }
# The empty line that separates [Unreleased] from the next version is not needed at the end of the file
if ($tail.Count -eq 0) { $section.RemoveAt($section.Count - 1) }
# The same backward range would repeat lines when [Unreleased] is the first line of the file
$head = if ($start -gt 0) { @($lines[0..($start - 1)]) } else { @() }
$result = $head + @($section) + $tail

# Step 4: write the changelog, then delete the merged fragments
if ($PSCmdlet.ShouldProcess($changelogPath, "Merge $($fragments.Count) fragment(s) into [Unreleased] and delete them"))
{
    [IO.File]::WriteAllText($changelogPath, (($result -join "`n") -replace "`r?`n", "`r`n") + "`r`n", [Text.UTF8Encoding]::new($false))
    # The changelog is written: a fragment left behind would be merged a second time by the next run.
    # Every deletion is tried, and the ones that fail are listed so that they are deleted by hand.
    $notDeleted = [Collections.Generic.List[string]]::new()
    foreach ($fragment in $fragments)
    {
        try { Remove-Item -LiteralPath $fragment.FullName -ErrorAction Stop }
        catch { $notDeleted.Add("$($fragment.Name) ($($_.Exception.Message))") }
    }
    if ($notDeleted.Count -gt 0)
    {
        Write-Host '::error::CHANGELOG.md is already updated, but these fragments could not be deleted. Delete them by hand and do NOT run the script again, or they would be merged twice:'
        $notDeleted | ForEach-Object { Write-Host "::error::  $_" }
        exit 1
    }
    Write-Host "Merged $($fragments.Count) fragment(s) into the [Unreleased] section of CHANGELOG.md."
}

exit 0
