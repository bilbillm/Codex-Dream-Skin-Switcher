[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$paths = @(
  (Join-Path ([Environment]::GetFolderPath('Desktop')) 'Codex 自定义主题.lnk'),
  (Join-Path ([Environment]::GetFolderPath('Programs')) 'Codex 自定义主题.lnk'),
  (Join-Path ([Environment]::GetFolderPath('Programs')) 'Codex 主题切换器.lnk')
)

foreach ($path in $paths) {
  if (Test-Path -LiteralPath $path -PathType Leaf) {
    Remove-Item -LiteralPath $path -Force
    Write-Host "Removed: $path"
  }
}

Write-Host 'Shortcut removal completed. Program files and Dream Skin runtime state were not deleted.' -ForegroundColor Green
