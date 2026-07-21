[CmdletBinding()]
param(
  [switch]$SkipDesktop,
  [switch]$SkipStartMenu
)

$ErrorActionPreference = 'Stop'

function Get-BundleRoot {
  $candidate = [System.IO.Path]::GetFullPath($PSScriptRoot)
  if (Test-Path -LiteralPath (Join-Path $candidate 'Codex自定义主题.exe') -PathType Leaf) {
    return $candidate
  }

  $parent = [System.IO.Path]::GetFullPath((Split-Path -Parent $candidate))
  if (Test-Path -LiteralPath (Join-Path $parent 'Codex自定义主题.exe') -PathType Leaf) {
    return $parent
  }

  throw 'Codex自定义主题.exe was not found beside this script or in its parent directory.'
}

function New-Shortcut {
  param(
    [Parameter(Mandatory = $true)][object]$Shell,
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][string]$Target,
    [string]$Arguments,
    [Parameter(Mandatory = $true)][string]$WorkingDirectory,
    [Parameter(Mandatory = $true)][string]$Description
  )

  $directory = Split-Path -Parent $Path
  if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
  }

  $shortcut = $Shell.CreateShortcut($Path)
  $shortcut.TargetPath = $Target
  $shortcut.Arguments = $Arguments
  $shortcut.WorkingDirectory = $WorkingDirectory
  $shortcut.Description = $Description
  $shortcut.IconLocation = "$Target,0"
  $shortcut.WindowStyle = 1
  $shortcut.Save()
}

$bundleRoot = Get-BundleRoot
$executable = Join-Path $bundleRoot 'Codex自定义主题.exe'
$shell = New-Object -ComObject WScript.Shell
$created = [System.Collections.Generic.List[string]]::new()

if (-not $SkipDesktop) {
  $desktopShortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Codex 自定义主题.lnk'
  New-Shortcut -Shell $shell -Path $desktopShortcut -Target $executable -Arguments '' `
    -WorkingDirectory $bundleRoot -Description '打开 Codex 自定义主题控制台'
  $created.Add($desktopShortcut)
}

if (-not $SkipStartMenu) {
  $programs = [Environment]::GetFolderPath('Programs')
  $consoleShortcut = Join-Path $programs 'Codex 自定义主题.lnk'
  $legacySwitcherShortcut = Join-Path $programs 'Codex 主题切换器.lnk'
  if (Test-Path -LiteralPath $legacySwitcherShortcut -PathType Leaf) {
    Remove-Item -LiteralPath $legacySwitcherShortcut -Force
  }
  New-Shortcut -Shell $shell -Path $consoleShortcut -Target $executable -Arguments '' `
    -WorkingDirectory $bundleRoot -Description '打开 Codex 自定义主题控制台'
  $created.Add($consoleShortcut)
}

$selfTest = Start-Process -FilePath $executable -ArgumentList '--self-test' -Wait -PassThru
if ($selfTest.ExitCode -ne 0) {
  throw "Shortcut installation completed, but the application self-test failed with exit code $($selfTest.ExitCode)."
}

Write-Host 'Codex Dream Skin Switcher shortcuts installed:' -ForegroundColor Green
$created | ForEach-Object { Write-Host "  $_" }
Write-Host "Installation directory: $bundleRoot"
