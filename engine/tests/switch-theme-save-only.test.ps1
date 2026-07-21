[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) "codex-theme-save-only-$PID-$([guid]::NewGuid().ToString('N'))"
$bundleRoot = Join-Path $temporaryRoot 'bundle'
$stateRoot = Join-Path $temporaryRoot 'state'
$originalLocalAppData = $env:LOCALAPPDATA

try {
  New-Item -ItemType Directory -Path $bundleRoot, $stateRoot -Force | Out-Null
  Copy-Item -LiteralPath (Join-Path $repoRoot 'engine') -Destination $bundleRoot -Recurse -Force
  Copy-Item -LiteralPath (Join-Path $repoRoot 'themes') -Destination $bundleRoot -Recurse -Force
  Copy-Item -LiteralPath (Join-Path $repoRoot 'src\CodexThemeSwitcher\switch-theme.ps1') -Destination $bundleRoot -Force

  $env:LOCALAPPDATA = $stateRoot
  $script = Join-Path $bundleRoot 'switch-theme.ps1'
  $theme = Join-Path $bundleRoot 'themes\Angelina Gravity Field'
  $output = @(& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -ThemeDirectory $theme -SaveOnly)
  if ($LASTEXITCODE -ne 0) { throw "Save-only theme activation failed with exit code $LASTEXITCODE." }
  $result = ($output -join "`n") | ConvertFrom-Json
  if (-not $result.pass -or -not $result.savedOnly -or $result.id -cne 'preset-angelina-gravity-field') {
    throw 'Save-only theme activation returned an unexpected result.'
  }

  $activeThemePath = Join-Path $stateRoot 'CodexDreamSkin\active-theme\theme.json'
  if (-not (Test-Path -LiteralPath $activeThemePath -PathType Leaf)) {
    throw 'Save-only theme activation did not write the active theme.'
  }
  $activeJson = [System.IO.File]::ReadAllText($activeThemePath, [System.Text.Encoding]::UTF8)
  $active = $activeJson | ConvertFrom-Json
  if ($active.id -cne 'preset-angelina-gravity-field') {
    throw 'Save-only theme activation wrote the wrong active theme.'
  }
  if (Test-Path -LiteralPath (Join-Path $stateRoot 'CodexDreamSkin\state.json')) {
    throw 'Save-only theme activation unexpectedly required a Dream Skin session.'
  }

  Write-Host 'PASS: save-only theme activation persists an offline theme without a CDP session.'
}
finally {
  if ($null -eq $originalLocalAppData) {
    Remove-Item Env:LOCALAPPDATA -ErrorAction SilentlyContinue
  } else {
    $env:LOCALAPPDATA = $originalLocalAppData
  }
  Remove-Item -LiteralPath $temporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
}
