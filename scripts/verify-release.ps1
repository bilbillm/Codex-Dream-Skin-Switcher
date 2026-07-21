[CmdletBinding()]
param(
  [string]$Version = '0.1.0'
)

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$artifactsRoot = Join-Path $repoRoot 'artifacts'
$zipName = "Codex-Dream-Skin-Switcher-v$Version-win-x64.zip"
$zipPath = Join-Path $artifactsRoot $zipName
$hashPath = Join-Path $artifactsRoot 'SHA256SUMS.txt'

if (-not (Test-Path -LiteralPath $zipPath -PathType Leaf)) { throw "Missing release ZIP: $zipPath" }
if (-not (Test-Path -LiteralPath $hashPath -PathType Leaf)) { throw "Missing checksum file: $hashPath" }

$expected = ((Get-Content -Raw -LiteralPath $hashPath).Trim() -split '\s+')[0]
$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath).Hash.ToLowerInvariant()
if ($expected -cne $actual) { throw "SHA-256 mismatch. Expected $expected, got $actual" }

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
  $names = @($archive.Entries | ForEach-Object { $_.FullName.Replace('/', '\') })
  $required = @(
    'CodexThemeSwitcher.exe',
    'switch-theme.ps1',
    '安装快捷方式.ps1',
    '卸载快捷方式.ps1',
    'README.md',
    'README.en.md',
    'engine\scripts\start-dream-skin.ps1',
    'engine\scripts\injector.mjs',
    'themes\Angelina Gravity Field\theme.json',
    'themes\Angelina Midnight Gravity\theme.json'
  )
  foreach ($name in $required) {
    if ($names -cnotcontains $name) { throw "Release ZIP is missing: $name" }
  }
} finally {
  $archive.Dispose()
}

$secretPatterns = @(
  'sk-[A-Za-z0-9_-]{32,}',
  'gh[pousr]_[A-Za-z0-9]{30,}',
  'Bearer\s+[A-Za-z0-9._-]{30,}'
)
$textFiles = Get-ChildItem -Recurse -File -LiteralPath $repoRoot | Where-Object {
  $_.FullName -notlike "$artifactsRoot*" -and $_.Extension -in @('.cs', '.ps1', '.mjs', '.js', '.json', '.md', '.txt', '.yaml', '.css')
}
foreach ($file in $textFiles) {
  $content = Get-Content -Raw -LiteralPath $file.FullName
  foreach ($pattern in $secretPatterns) {
    if ($content -match $pattern) { throw "Potential credential pattern found in $($file.FullName)" }
  }
}

Write-Host "Release verification passed: $zipName" -ForegroundColor Green
Write-Host "SHA-256: $actual"
