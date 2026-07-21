[CmdletBinding()]
param(
  [string]$Destination = "$env:USERPROFILE\Documents\GitHub\Codex-Dream-Skin-Switcher"
)

$ErrorActionPreference = 'Stop'

function Invoke-NativeQuiet {
  param([Parameter(Mandatory = $true)][scriptblock]$Command)
  $previousPreference = $ErrorActionPreference
  try {
    $ErrorActionPreference = 'Continue'
    & $Command *> $null
    return $LASTEXITCODE
  } finally {
    $ErrorActionPreference = $previousPreference
  }
}

function Invoke-NativeCapture {
  param([Parameter(Mandatory = $true)][scriptblock]$Command)
  $previousPreference = $ErrorActionPreference
  try {
    $ErrorActionPreference = 'Continue'
    $output = @(& $Command 2>$null)
    $exitCode = $LASTEXITCODE
  } finally {
    $ErrorActionPreference = $previousPreference
  }
  return [pscustomobject]@{ ExitCode = $exitCode; Output = $output }
}

$repoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$destinationRoot = [System.IO.Path]::GetFullPath($Destination)
$expectedParent = [System.IO.Path]::GetFullPath("$env:USERPROFILE\Documents\GitHub").TrimEnd('\') + '\'
if (-not $destinationRoot.StartsWith($expectedParent, [System.StringComparison]::OrdinalIgnoreCase)) {
  throw "Destination must remain under the current user's Documents\GitHub directory: $destinationRoot"
}
if ($destinationRoot -ceq $repoRoot) {
  Write-Host 'Repository is already at the requested destination.'
  exit 0
}

if (-not (Test-Path -LiteralPath $destinationRoot -PathType Container)) {
  New-Item -ItemType Directory -Path $destinationRoot -Force | Out-Null
}

$destinationGit = Join-Path $destinationRoot '.git'
if (Test-Path -LiteralPath $destinationGit -PathType Container) {
  $headCheck = Invoke-NativeQuiet { git -C $destinationRoot rev-parse --verify HEAD }
  if ($headCheck -eq 0) {
    throw 'Destination already contains committed Git history; refusing to replace it.'
  }
  $resolvedGit = [System.IO.Path]::GetFullPath($destinationGit)
  $expectedGit = $destinationRoot.TrimEnd('\') + '\.git'
  if ($resolvedGit -cne $expectedGit) { throw "Unexpected .git path: $resolvedGit" }
  Remove-Item -LiteralPath $resolvedGit -Recurse -Force
}

$temporaryZip = Join-Path ([System.IO.Path]::GetTempPath()) "codex-dream-skin-switcher-$PID-$([guid]::NewGuid().ToString('N')).zip"
try {
  $archiveExit = Invoke-NativeQuiet { git -C $repoRoot archive --format=zip --output=$temporaryZip HEAD }
  if ($archiveExit -ne 0) { throw 'git archive failed.' }
  Expand-Archive -LiteralPath $temporaryZip -DestinationPath $destinationRoot -Force
} finally {
  Remove-Item -LiteralPath $temporaryZip -Force -ErrorAction SilentlyContinue
}

Copy-Item -LiteralPath (Join-Path $repoRoot '.git') -Destination $destinationRoot -Recurse -Force
$addExit = Invoke-NativeQuiet { git -C $destinationRoot add --all }
if ($addExit -ne 0) { throw 'Failed to refresh the destination Git index.' }

$destinationTreeResult = Invoke-NativeCapture { git -C $destinationRoot write-tree }
if ($destinationTreeResult.ExitCode -ne 0) { throw 'Failed to read the synchronized Git tree.' }
$destinationTree = ($destinationTreeResult.Output -join "`n").Trim()
$expectedTreeResult = Invoke-NativeCapture { git -C $destinationRoot rev-parse "HEAD^{tree}" }
if ($expectedTreeResult.ExitCode -ne 0) { throw 'Failed to read the expected Git tree.' }
$expectedTree = ($expectedTreeResult.Output -join "`n").Trim()
if ($destinationTree -cne $expectedTree) {
  [void](Invoke-NativeQuiet { git -C $destinationRoot reset --mixed --quiet HEAD })
  throw 'Destination contains files that differ from the synchronized commit.'
}

$statusResult = Invoke-NativeCapture { git -C $destinationRoot status --short }
if ($statusResult.ExitCode -ne 0) { throw 'Failed to verify the synchronized repository.' }
$status = @($statusResult.Output)
if ($status.Count -gt 0) {
  $status | Write-Host
  throw 'Destination contains files that differ from the synchronized commit.'
}
Write-Host "Project synchronized to: $destinationRoot" -ForegroundColor Green
