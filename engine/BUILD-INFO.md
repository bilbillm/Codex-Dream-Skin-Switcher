# Build information

- Package: Angelina Gravity Field for ChatGPT Codex
- Package version: `3.1.4-angelina`
- Build date: 2026-07-21
- Upstream: `Fei-Away/Codex-Dream-Skin`
- Upstream branch: `main`
- Upstream commit: `e776fa6d5361a2bdd5c1614674397681e7b00874`
- Target verified locally: `OpenAI.Codex_26.715.7063.0_x64__2p2nqsd0c76g0`
- Target executable layout: `app\ChatGPT.exe`
- Local Node.js used for validation: `v24.7.0`

## Validation

- `windows/tests/run-tests.ps1`: PASS
- JavaScript syntax checks: PASS
- PowerShell parser checks: PASS
- Bright Angelina, Angelina Midnight Gravity, Arina Hashimoto, and Gothic Void Crusade payload construction: PASS
- Theme-store four-preset initialization, dual-background save/switch, and dark-preset coverage: PASS
- Renderer variant, route cleanup, collapsed-sidebar and auxiliary-window coverage: PASS
- Asset dimensions and metadata limits: PASS
- Release inventory and secret-pattern scan: PASS
- Previous live hot reapply baseline on `OpenAI.Codex_26.715.4045.0`: PASS
- Installed `OpenAI.Codex_26.715.7063.0` renderer-source verification of the `data-codex-composer-request-navigation` request-card anchor: PASS
- Static bright renderer verification (`3.1.4-angelina`, transparent task header, 18px frosted left/right/bottom shell surfaces, bubble-matched renderer menus, pinned-summary card and composer-matched multi-question response card): PASS
- Live renderer menu verification (task actions, project/profile menus, Radix menu geometry, hover/focus states and pointer events preserved): PASS
- Live right/bottom panel verification (glass shell and chrome, native resize handles, terminal background and interaction preserved, light and dark presets): PASS
- Live edit-form selector probe (same bubble surface, border, shadow, blur, text, and caret variables as the user bubble): PASS
- Live bright task, bright side-panel-open, midnight task, and midnight home screenshot review: PASS
- Midnight 2048 x 1152 home image and 1280 x 720 task image dimensions/rendering: PASS

The update was hot reapplied to the active verified CDP session without restarting ChatGPT/Codex. Fresh installations should still run `verify-dream-skin.ps1` after first launch for local Store process, DOM, and screenshot signoff.
