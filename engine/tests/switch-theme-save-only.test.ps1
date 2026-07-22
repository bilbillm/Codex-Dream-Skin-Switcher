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
  $theme = Join-Path $bundleRoot 'themes\test-theme'
  New-Item -ItemType Directory -Path $theme -Force | Out-Null
  $pngHeader = New-Object byte[] 24
  [byte[]](0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a) | ForEach-Object -Begin { $i = 0 } -Process { $pngHeader[$i++] = $_ }
  $pngHeader[8] = 0; $pngHeader[9] = 0; $pngHeader[10] = 0; $pngHeader[11] = 13
  [byte[]](0x49, 0x48, 0x44, 0x52) | ForEach-Object -Begin { $i = 12 } -Process { $pngHeader[$i++] = $_ }
  $pngHeader[19] = 1; $pngHeader[23] = 1
  [System.IO.File]::WriteAllBytes((Join-Path $theme 'fixture.png'), $pngHeader)
  [System.IO.File]::WriteAllText((Join-Path $theme 'theme.json'), "{`"id`":`"test-theme`",`"name`":`"Test Theme`",`"image`":`"fixture.png`"}", [System.Text.UTF8Encoding]::new($false))
  Copy-Item -LiteralPath (Join-Path $repoRoot 'src\CodexThemeSwitcher\switch-theme.ps1') -Destination $bundleRoot -Force

  $env:LOCALAPPDATA = $stateRoot
  $script = Join-Path $bundleRoot 'switch-theme.ps1'
  $output = @(& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -ThemeDirectory $theme -SaveOnly)
  if ($LASTEXITCODE -ne 0) { throw "Save-only theme activation failed with exit code $LASTEXITCODE." }
  $result = ($output -join "`n") | ConvertFrom-Json
  if (-not $result.pass -or -not $result.savedOnly -or $result.id -cne 'test-theme') {
    throw 'Save-only theme activation returned an unexpected result.'
  }

  $activeThemePath = Join-Path $stateRoot 'CodexDreamSkin\active-theme\theme.json'
  if (-not (Test-Path -LiteralPath $activeThemePath -PathType Leaf)) {
    throw 'Save-only theme activation did not write the active theme.'
  }
  $activeJson = [System.IO.File]::ReadAllText($activeThemePath, [System.Text.Encoding]::UTF8)
  $active = $activeJson | ConvertFrom-Json
  if ($active.id -cne 'test-theme') {
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
