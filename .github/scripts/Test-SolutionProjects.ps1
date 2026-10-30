# ┌──────────────┬──────────────────────────────────────────────────────────────┐
# │ File         │ .github/scripts/Test-SolutionProjects.ps1                    │
# │ Purpose      │ Fails when the solution and the projects on disk no longer   │
# │              │ match, so that no project is silently left out of the CI     │
# │              │ build and tests.                                             │
# │ Rules        │ 1. Every .csproj under src/ and test/ is in the solution.    │
# │              │ 2. Every project of the solution exists on disk.             │
# │ Usage        │ pwsh .github/scripts/Test-SolutionProjects.ps1               │
# │ Exit code    │ 0 when they match, 1 otherwise.                              │
# └──────────────┴──────────────────────────────────────────────────────────────┘

[CmdletBinding()]
param(
    # Root of the repository; by default, two levels above this script
    [string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

# Paths are compared in one form: relative, forward slashes (-in and -notin ignore case)
function ConvertTo-NormalizedPath([string] $Path)
{
    return $Path.Replace('\', '/')
}

$solutionPath = Join-Path $RepositoryRoot 'OpenSpaceIA.slnx'

# Projects declared in the solution
[xml] $solution = Get-Content -Raw $solutionPath
$solutionProjects = @($solution.SelectNodes('//Project') | ForEach-Object { ConvertTo-NormalizedPath $_.Path })

# Projects present on disk (a missing src/ or test/ folder simply holds no project)
$diskProjects = @(
    foreach ($folder in 'src', 'test')
    {
        $path = Join-Path $RepositoryRoot $folder
        if (Test-Path $path)
        {
            Get-ChildItem -Path $path -Filter '*.csproj' -Recurse -File |
                ForEach-Object { ConvertTo-NormalizedPath ([IO.Path]::GetRelativePath($RepositoryRoot, $_.FullName)) }
        }
    }
)

$errors = [Collections.Generic.List[string]]::new()

# Rule 1: a project on disk but not in the solution is never built nor tested
foreach ($project in $diskProjects | Where-Object { $_ -notin $solutionProjects })
{
    $errors.Add("Not in OpenSpaceIA.slnx: $project")
}

# Rule 2: a project of the solution that does not exist breaks the build for every contributor
foreach ($project in $solutionProjects | Where-Object { -not (Test-Path (Join-Path $RepositoryRoot $_)) })
{
    $errors.Add("In OpenSpaceIA.slnx but not on disk: $project")
}

if ($errors.Count -gt 0)
{
    $errors | ForEach-Object { Write-Host "::error::$_" }
    exit 1
}

Write-Host "OpenSpaceIA.slnx matches the projects on disk ($($solutionProjects.Count) project(s))."
exit 0
