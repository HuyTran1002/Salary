<#
./scripts/release.ps1

Usage:
  # Increment patch and publish
  .\scripts\release.ps1

  # Increment minor
  .\scripts\release.ps1 -Part minor

  # Use explicit version
  .\scripts\release.ps1 -Version 1.2.3.0

What it does:
  - Reads version from version.txt in repo root (format: Major.Minor.Build.Revision)
  - If -Version not provided, increments the requested part (patch/minor/major)
  - Writes updated version back to version.txt
  - Runs dotnet publish for SalaryCalculator.csproj with single-file, self-contained publish for win-x64
  - Sets MSBuild property `Version` so produced assembly reflects the version
  - Publishes output to Releases\<version> folder
#>

param(
    [string]$Version,
    [ValidateSet('patch','minor','major')]
    [string]$Part = 'patch'
)

Set-StrictMode -Version Latest
Push-Location -Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) | Out-Null
Push-Location -Path ".." | Out-Null

$versionFile = Join-Path -Path (Get-Location) -ChildPath 'version.txt'
if (-not (Test-Path $versionFile)) {
    Write-Error "version.txt not found in repo root"
    Exit 1
}

$current = (Get-Content $versionFile -Raw).Trim()
if (-not $Version) {
    # bump
    $parts = $current.Split('.') | ForEach-Object {[int]$_}
    if ($parts.Count -lt 4) { $parts = $parts + (0..(3-$parts.Count) | ForEach-Object {0}) }
    switch ($Part) {
        'patch' { $parts[3] = $parts[3] + 1 }
        'minor' { $parts[2] = $parts[2] + 1; $parts[3] = 0 }
        'major' { $parts[0] = $parts[0] + 1; $parts[2] = 0; $parts[3] = 0 }
    }
    $newVersion = ($parts -join '.')
} else {
    $newVersion = $Version
}

Write-Host "Version: $current -> $newVersion"
Set-Content -Path $versionFile -Value $newVersion -Encoding UTF8

$proj = 'SalaryCalculator.csproj'
if (-not (Test-Path $proj)) { Write-Error "Project file $proj not found"; Exit 1 }

# Update Version tags inside csproj so source and published builds are consistent
try {
    $projXml = [xml](Get-Content $proj -Raw)
    $ns = @{ }
    # Ensure PropertyGroup exists
    $pg = $projXml.Project.PropertyGroup | Select-Object -First 1
    if (-not $pg) { $pg = $projXml.CreateElement('PropertyGroup'); $projXml.Project.AppendChild($pg) | Out-Null }

    # Helper to set or add element
    function Ensure-ElementValue($parent, $name, $value) {
        $node = $parent.SelectSingleNode($name)
        if ($node -eq $null) {
            $n = $projXml.CreateElement($name)
            $n.InnerText = $value
            $parent.AppendChild($n) | Out-Null
        } else {
            $node.InnerText = $value
        }
    }

    $backup = "$proj.bak"
    Copy-Item $proj $backup -Force
    Ensure-ElementValue $pg 'Version' $newVersion
    Ensure-ElementValue $pg 'AssemblyVersion' $newVersion
    Ensure-ElementValue $pg 'FileVersion' $newVersion
    $projXml.Save($proj)
    Write-Host "Updated $proj Version tags to $newVersion (backup at $backup)"
}
catch {
    Write-Warning "Could not update csproj: $_";
}

$outDir = Join-Path -Path (Get-Location) -ChildPath ("Releases\$newVersion")
if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }
New-Item -ItemType Directory -Path $outDir | Out-Null

Write-Host "Publishing single-file self-contained release to: $outDir"

$publishArgs = @(
    'publish', $proj,
    '-c','Release',
    '-r','win-x64',
    '--self-contained','true',
    '/p:PublishSingleFile=true',
    "/p:Version=$newVersion",
    '/p:PublishTrimmed=false',
    '-o', $outDir
)

Write-Host 'dotnet' ($publishArgs -join ' ')
dotnet @publishArgs

if ($LASTEXITCODE -ne 0) { Write-Error 'dotnet publish failed'; Exit $LASTEXITCODE }

Write-Host "Release published to: $outDir"
Pop-Location
Pop-Location
