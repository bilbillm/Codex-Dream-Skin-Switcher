[CmdletBinding()]
param(
  [string]$Repository = 'bilbillm/Codex-Dream-Skin-Switcher',
  [string]$Version = '0.1.0'
)

$ErrorActionPreference = 'Stop'

function Invoke-Native {
  param(
    [Parameter(Mandatory = $true)][scriptblock]$Command,
    [switch]$Quiet
  )
  $previousPreference = $ErrorActionPreference
  try {
    $ErrorActionPreference = 'Continue'
    if ($Quiet) {
      & $Command *> $null
    } else {
      & $Command | Out-Host
    }
    $exitCode = $LASTEXITCODE
  } finally {
    $ErrorActionPreference = $previousPreference
  }
  return $exitCode
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
$tag = "v$Version"
$sshUrl = "git@github.com:$Repository.git"
$zipPath = Join-Path $repoRoot "artifacts\Codex-Dream-Skin-Switcher-v$Version-win-x64.zip"
$hashPath = Join-Path $repoRoot 'artifacts\SHA256SUMS.txt'
$notesPath = Join-Path $repoRoot "docs\releases\v$Version.md"

foreach ($path in @($zipPath, $hashPath, $notesPath)) {
  if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    throw "Missing release input: $path"
  }
}

& (Join-Path $PSScriptRoot 'verify-release.ps1') -Version $Version

$statusResult = Invoke-NativeCapture { git -C $repoRoot status --porcelain }
if ($statusResult.ExitCode -ne 0) { throw 'Failed to inspect the local Git worktree.' }
if (@($statusResult.Output).Count -gt 0) {
  throw 'The local Git worktree is not clean. Commit or remove pending files before publishing.'
}
$headResult = Invoke-NativeCapture { git -C $repoRoot rev-parse HEAD }
$tagResult = Invoke-NativeCapture { git -C $repoRoot rev-parse "$tag^{commit}" }
if ($headResult.ExitCode -ne 0 -or $tagResult.ExitCode -ne 0) {
  throw "Local commit or tag is missing: $tag"
}
$headCommit = ($headResult.Output -join "`n").Trim()
$tagCommit = ($tagResult.Output -join "`n").Trim()
if ($headCommit -cne $tagCommit) { throw "Local tag $tag does not point to HEAD." }

$authExit = Invoke-Native { gh auth status --hostname github.com }
if ($authExit -ne 0) {
  throw 'GitHub CLI is not authenticated. Run gh auth login in this Windows user session first.'
}

$repositoryExists = (Invoke-Native -Quiet { gh repo view $Repository --json nameWithOwner }) -eq 0
if (-not $repositoryExists) {
  $createExit = Invoke-Native { gh repo create $Repository --public `
    --description 'Windows GUI for previewing, hot-switching, and one-click launching Codex Dream Skin themes.' `
    --disable-wiki }
  if ($createExit -ne 0) { throw 'GitHub repository creation failed.' }
}

$originResult = Invoke-NativeCapture { git -C $repoRoot remote get-url origin }
if ($originResult.ExitCode -ne 0) {
  $remoteExit = Invoke-Native { git -C $repoRoot remote add origin $sshUrl }
  if ($remoteExit -ne 0) { throw 'Failed to add the origin remote.' }
} else {
  $origin = ($originResult.Output -join "`n").Trim()
  if ($origin -cne $sshUrl) {
    Write-Warning "Leaving the existing origin unchanged; publishing uses SSH directly: $sshUrl"
  }
}

$pushMainExit = Invoke-Native { git -C $repoRoot push $sshUrl main }
if ($pushMainExit -ne 0) { throw 'SSH push of main failed.' }
$pushTagExit = Invoke-Native { git -C $repoRoot push $sshUrl "refs/tags/$tag" }
if ($pushTagExit -ne 0) { throw "SSH push of $tag failed." }

$metadataExit = Invoke-Native { gh repo edit $Repository --enable-issues --enable-wiki=false `
  --visibility public `
  --accept-visibility-change-consequences `
  --add-topic codex `
  --add-topic dream-skin `
  --add-topic theme-switcher `
  --add-topic windows `
  --add-topic winforms `
  --add-topic powershell `
  --add-topic glassmorphism }
if ($metadataExit -ne 0) { throw 'Repository metadata update failed.' }

$releaseExists = (Invoke-Native -Quiet { gh release view $tag --repo $Repository }) -eq 0
if (-not $releaseExists) {
  $releaseExit = Invoke-Native { gh release create $tag `
    --repo $Repository `
    --title "$tag - First public release / 首个公开版本" `
    --notes-file $notesPath `
    --verify-tag }
  if ($releaseExit -ne 0) { throw "GitHub Release $tag creation failed." }
}

$uploadExit = Invoke-Native { gh release upload $tag $zipPath $hashPath --repo $Repository --clobber }
if ($uploadExit -ne 0) { throw "GitHub Release $tag asset upload failed." }

$repoViewExit = Invoke-Native { gh repo view $Repository --json nameWithOwner,url,visibility,defaultBranchRef }
if ($repoViewExit -ne 0) { throw 'Final repository verification failed.' }
$releaseViewExit = Invoke-Native { gh release view $tag --repo $Repository --json name,tagName,url,isDraft,isPrerelease,assets }
if ($releaseViewExit -ne 0) { throw 'Final Release verification failed.' }
