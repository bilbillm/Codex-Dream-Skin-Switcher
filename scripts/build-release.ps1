[CmdletBinding()]
param(
  [string]$Version = '0.1.0',
  [string]$RuntimeFrameworkVersion = '10.0.10',
  [switch]$FrameworkDependent
)

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$artifactsRoot = Join-Path $repoRoot 'artifacts'
$publishRoot = Join-Path $artifactsRoot 'publish'
$stagingRoot = Join-Path $artifactsRoot "Codex-Dream-Skin-Switcher-v$Version-win-x64"
$zipPath = "$stagingRoot.zip"
$hashPath = Join-Path $artifactsRoot 'SHA256SUMS.txt'

function Assert-ChildPath {
  param([Parameter(Mandatory = $true)][string]$Path)
  $full = [System.IO.Path]::GetFullPath($Path)
  $prefix = $repoRoot.TrimEnd('\') + '\'
  if (-not $full.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to modify a path outside the repository: $full"
  }
  return $full
}

foreach ($path in @($publishRoot, $stagingRoot, $zipPath, $hashPath)) {
  $safePath = Assert-ChildPath -Path $path
  if (Test-Path -LiteralPath $safePath) {
    Remove-Item -Recurse -Force -LiteralPath $safePath
  }
}

New-Item -ItemType Directory -Path $publishRoot, $stagingRoot -Force | Out-Null
$project = Join-Path $repoRoot 'src\CodexThemeSwitcher\CodexThemeSwitcher.csproj'
$selfContained = if ($FrameworkDependent) { 'false' } else { 'true' }
$runtimeProperties = @(
  '-p:RestoreIgnoreFailedSources=true',
  '-p:NuGetAudit=false'
)
if (-not $FrameworkDependent) {
  $runtimeProperties += "-p:RuntimeFrameworkVersion=$RuntimeFrameworkVersion"
}

& dotnet publish $project -c Release -r win-x64 --self-contained $selfContained `
  @runtimeProperties `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -p:Version=$Version `
  -o $publishRoot
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

Copy-Item -LiteralPath (Join-Path $publishRoot 'CodexThemeSwitcher.exe') -Destination $stagingRoot
Copy-Item -Recurse -LiteralPath (Join-Path $repoRoot 'engine') -Destination $stagingRoot
Copy-Item -Recurse -LiteralPath (Join-Path $repoRoot 'themes') -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $repoRoot 'src\CodexThemeSwitcher\switch-theme.ps1') -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts\install-shortcuts.ps1') `
  -Destination (Join-Path $stagingRoot '安装快捷方式.ps1')
Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts\uninstall-shortcuts.ps1') `
  -Destination (Join-Path $stagingRoot '卸载快捷方式.ps1')
foreach ($name in @('README.md', 'README.en.md', 'LICENSE', 'THIRD-PARTY-NOTICES.md', 'CHANGELOG.md')) {
  Copy-Item -LiteralPath (Join-Path $repoRoot $name) -Destination $stagingRoot
}

Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
$hash = Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath
$line = "$($hash.Hash.ToLowerInvariant())  $([System.IO.Path]::GetFileName($zipPath))`r`n"
[System.IO.File]::WriteAllText($hashPath, $line, [System.Text.UTF8Encoding]::new($false))

Write-Host "Release bundle: $zipPath" -ForegroundColor Green
Write-Host "SHA-256: $($hash.Hash)"
