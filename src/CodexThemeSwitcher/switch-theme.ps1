[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$ThemeDirectory,
  [switch]$ValidateOnly
)

$ErrorActionPreference = 'Stop'
$appRoot = $PSScriptRoot
$themeRoot = Join-Path $appRoot 'themes'
$engineRoot = Join-Path $appRoot 'engine'
$common = Join-Path $engineRoot 'scripts\common-windows.ps1'
$themeTools = Join-Path $engineRoot 'scripts\theme-windows.ps1'
if (-not (Test-Path -LiteralPath $common -PathType Leaf) -or
  -not (Test-Path -LiteralPath $themeTools -PathType Leaf)) {
  throw '切换器运行引擎不完整，请重新部署软件。'
}

. $common
. $themeTools

$root = [System.IO.Path]::GetFullPath($themeRoot).TrimEnd('\') + '\'
$directory = [System.IO.Path]::GetFullPath($ThemeDirectory)
if (-not $directory.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
  throw '主题必须位于本软件的 themes 文件夹内。'
}

$loaded = Read-DreamSkinTheme -ThemeDirectory $directory
if ($ValidateOnly) {
  [pscustomobject]@{
    pass = $true
    id = "$($loaded.Theme.id)"
    name = "$($loaded.Theme.name)"
    appearance = "$($loaded.Theme.appearance)"
    image = $loaded.ImagePath
    taskImage = $loaded.TaskImagePath
  } | ConvertTo-Json -Compress
  exit 0
}

$stateRoot = Join-Path $env:LOCALAPPDATA 'CodexDreamSkin'
$theme = $loaded.Theme | ConvertTo-Json -Depth 8 | ConvertFrom-Json
$active = Set-DreamSkinActiveTheme -ImagePath $loaded.ImagePath -TaskImagePath $loaded.TaskImagePath `
  -Theme $theme -StateRoot $stateRoot
$null = Set-DreamSkinPaused -Paused $false -StateRoot $stateRoot

$paths = Get-DreamSkinThemePaths -StateRoot $stateRoot
$state = $null
try { $state = Read-DreamSkinState -Path $paths.State } catch { $state = $null }
$port = if ($null -ne $state -and $state.port) { [int]$state.port } else { 9335 }
Assert-DreamSkinPort -Port $port
$codex = if ($null -ne $state) { Get-DreamSkinCodexInstallFromState -State $state } else { $null }
if ($null -eq $codex) { $codex = Get-DreamSkinCodexInstall }
$identity = Get-DreamSkinVerifiedCdpIdentity -Port $port -Codex $codex
if ($null -eq $identity) {
  throw '没有可连接的 Codex 热切换会话。请点击“启动热切换服务”。'
}

$node = Get-DreamSkinNodeRuntime
$injector = Join-Path $engineRoot 'scripts\injector.mjs'
$apply = Invoke-DreamSkinNative -FilePath $node.Path -ArgumentList @(
  $injector,
  '--once',
  '--port', "$port",
  '--browser-id', $identity.BrowserId,
  '--theme-dir', $paths.Active,
  '--timeout-ms', '30000'
) -DiscardStderr
if ($apply.ExitCode -ne 0) {
  $detail = (($apply.Output -join "`n").Trim() -split "`n" | Select-Object -Last 1)
  throw "主题已保存，但热应用失败：$detail"
}

[pscustomobject]@{
  pass = $true
  id = "$($active.Theme.id)"
  name = "$($active.Theme.name)"
  appearance = "$($active.Theme.appearance)"
  port = $port
  browserId = $identity.BrowserId
} | ConvertTo-Json -Compress
