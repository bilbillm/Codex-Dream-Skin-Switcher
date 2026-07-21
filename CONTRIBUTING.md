# Contributing / 贡献指南

## Before opening a pull request

1. Keep changes scoped to one behavior or document set.
2. Preserve Dream Skin's process, path, rollback, and loopback security boundaries.
3. Do not modify WindowsApps, `app.asar`, signatures, credentials, conversations, or project data.
4. Add or update tests in proportion to the change.
5. Update both `README.md` and `README.en.md` when public behavior changes.
6. Update `CHANGELOG.md` for user-visible changes.

## Theme contributions

A contributed theme must:

- use a stable unique ID;
- pass theme validation;
- contain only local data assets;
- document source and redistribution permission;
- avoid trademarks or character art without a clear legal basis;
- include light/dark contrast and home/task screenshots;
- preserve terminal and input interactions.

Put detailed provenance beside the theme or in `THIRD-PARTY-NOTICES.md`.

## Code style

- Match the existing C# and PowerShell style.
- Prefer structured APIs and complete-path validation.
- Keep comments for non-obvious invariants rather than narrating simple statements.
- Use Windows PowerShell 5.1-compatible syntax in runtime `.ps1` files unless a script explicitly targets PowerShell 7.
- Do not introduce third-party GUI packages unless they remove substantial complexity.

## Test commands

```powershell
dotnet build .\src\CodexThemeSwitcher\CodexThemeSwitcher.csproj -c Release
node --test .\engine\tests\*.test.mjs
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\engine\tests\run-tests.ps1
.\scripts\build-release.ps1
.\scripts\verify-release.ps1
```

Visual changes must be checked in light and dark themes at normal and high DPI. Capture the exact UI state affected by the change, including open menus, dialogs, panels, or pinned-summary cards.

## Commit and pull request content

Explain:

- what changed;
- why the previous behavior was insufficient;
- security or compatibility impact;
- test evidence;
- screenshots for visual changes;
- any remaining limitations.
