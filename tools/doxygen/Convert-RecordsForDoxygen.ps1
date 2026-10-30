# ┌──────────────┬──────────────────────────────────────────────────────────────┐
# │ File         │ tools/doxygen/Convert-RecordsForDoxygen.ps1                  │
# │ Purpose      │ Input filter of Doxygen for the C# files: Doxygen 1.18 does  │
# │              │ not know the "record" keyword and reads each record as a     │
# │              │ property, so the documented records get no page.             │
# │ Rules        │ 1. The file is printed for Doxygen only; the source file is  │
# │              │    never changed.                                            │
# │              │ 2. "record struct" is read as "struct"; "record" after a     │
# │              │    modifier (public, internal, sealed, abstract, partial) is │
# │              │    read as "class". The word "record" anywhere else (in a    │
# │              │    comment, for example) is left as it is.                   │
# │              │ 3. The lines are kept one for one, so that the line numbers  │
# │              │    of the warnings stay right.                               │
# │ Usage        │ Called by Doxygen (FILTER_PATTERNS in the Doxyfile), with    │
# │              │ the path of one C# file; the result goes to the output.      │
# │ Exit code    │ 0 on success; any error stops the script.                    │
# └──────────────┴──────────────────────────────────────────────────────────────┘

param(
    # Path of the C# file that Doxygen reads
    [Parameter(Mandatory)]
    [string] $Path
)

$ErrorActionPreference = 'Stop'

# Step 1: read the file as UTF-8, the encoding of the repository (.editorconfig)
$text = [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8)

# Step 2: present the records as the structs and classes that Doxygen knows
$text = $text -replace '\brecord\s+struct\b', 'struct'
$text = $text -replace '\b(public|internal|sealed|abstract|partial)\s+record\b', '$1 class'

# Step 3: give the result to Doxygen in UTF-8 (INPUT_ENCODING), whatever the code page of the console
$stream = [Console]::OpenStandardOutput()
$bytes = (New-Object System.Text.UTF8Encoding $false).GetBytes($text)
$stream.Write($bytes, 0, $bytes.Length)
$stream.Flush()
