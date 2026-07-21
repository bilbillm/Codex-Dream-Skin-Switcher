# Changelog

All notable changes to this project are documented here. Versions follow Semantic Versioning.

## [0.1.0] - 2026-07-20

### Added

- Windows Forms theme catalog with home/task preview tabs.
- Hot switching through the verified Dream Skin loopback CDP session.
- One-click launcher mode with healthy-session fast path and automatic Codex activation.
- Cold-start recovery delegates loopback port selection to the engine, so an unverified listener on 9335 no longer blocks startup.
- Theme folder import with traversal, reserved-name, and reparse-point protections.
- Angelina Gravity Field light theme and Angelina Midnight Gravity dark theme.
- Frosted-glass sidebars, bottom panels, in-app secondary menus, and chat bubbles.
- Frosted top-right pinned-summary card with light/dark text, item-state, and divider treatment.
- Composer-matched frosted multi-question response card with white text, icons, options, and focus states.
- Desktop and Start Menu shortcut installer/uninstaller.
- Self-contained win-x64 release builder, release verifier, and SHA-256 manifest.
- Chinese-first bilingual documentation.

### Compatibility

- Verified against `OpenAI.Codex_26.715.7063.0_x64__2p2nqsd0c76g0` on Windows 11.
- Dream Skin runtime revision: `3.1.4-angelina`.

### Known limitations

- Native Electron application menus (`File/Edit/View/Help`) are not styled.
- DOM selectors may require updates after major Codex desktop releases.
- Windows only; the bundled launcher targets x64.
