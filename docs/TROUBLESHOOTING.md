# Troubleshooting / 故障排查

## First-response checklist / 首轮检查

Run these read-only commands in Windows PowerShell:

```powershell
$root = Join-Path $env:LOCALAPPDATA 'CodexDreamSkin'
Get-ChildItem -Force $root
Get-Content -Raw (Join-Path $root 'state.json')
Get-Content -Tail 80 (Join-Path $root 'injector-error.log')
Get-Content -Tail 80 (Join-Path $root 'verify.log')
Get-NetTCPConnection -LocalPort 9335 -ErrorAction SilentlyContinue
```

Before sharing output, remove Browser ID, user-specific paths, account details, conversation content, and any unrelated environment values.

## Service disconnected

1. Confirm Codex is running.
2. Confirm the PID in `state.json` exists.
3. Query `http://127.0.0.1:9335/json/version` locally.
4. If the endpoint is absent, open the control console, select a theme, and click “启动 Codex”.
5. If prompted to restart, save unsent input first.

Do not change the port until you know what owns 9335. Dream Skin refuses unverified listeners intentionally.

## Theme apply failure

Validate a theme without changing runtime state:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\switch-theme.ps1 `
  -ThemeDirectory '.\themes\Angelina Gravity Field' `
  -ValidateOnly
```

Common failures:

- missing `theme.json`;
- missing `id`, `name`, or `image`;
- absolute or escaping image path;
- missing image file;
- theme outside the bundle's `themes` root;
- no verified CDP session (use `-SaveOnly` to validate and save a theme for the next launch without CDP).

## Console remains busy

The console allows up to 45 seconds for Codex/CDP readiness. A first migration from an older watcher can take longer because the runtime verifies and stops the old Node process before creating a new one.

Check for an error dialog behind other windows. Then inspect:

```powershell
Get-CimInstance Win32_Process | Where-Object {
  $_.CommandLine -match 'start-dream-skin|injector.mjs'
} | Select-Object ProcessId, ParentProcessId, Name, CreationDate, CommandLine
```

Do not kill a PID solely because it appears in a stale state file. Verify Node executable, injector path, `--watch`, port, Browser ID, and process start time first.

## Codex update broke styling

1. Record the Codex package version:

   ```powershell
   Get-AppxPackage OpenAI.Codex | Select-Object Name, Version, PackageFullName
   ```

2. Run renderer verification:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass `
     -File .\engine\scripts\verify-dream-skin.ps1 `
     -ScreenshotPath "$env:TEMP\dream-skin-check.png"
   ```

3. Compare home, task, side-panel, modal/menu, terminal, and pinned-summary states.
4. Open an issue with the version, sanitized verify log, and screenshot.

Never patch WindowsApps or `app.asar` as a workaround.

## PowerShell 7 identity mismatch

The GUI intentionally invokes startup through Windows PowerShell 5.1. Running the same startup script directly with PowerShell 7 can make ISO JSON timestamps become `DateTime`, producing a localized string that fails exact identity comparison. Use:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\engine\scripts\start-dream-skin.ps1 -PromptRestart
```

## Logs remain empty

An empty `injector.log` is not automatically a failure. Use `state.json`, process existence, CDP identity, and `verify.log` together. `injector-error.log` should normally be empty.

## Full recovery

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\engine\scripts\restore-dream-skin.ps1 `
  -RestoreBaseTheme -PromptRestart
```

Run with `-Uninstall` only after reviewing what managed runtime and shortcuts it removes.
