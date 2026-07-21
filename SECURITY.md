# Security policy / 安全策略

## Supported versions

Security fixes are provided for the latest published release. During the `0.x` phase, users should upgrade to the newest release rather than expecting backports.

## Reporting a vulnerability

Do not open a public issue for a vulnerability involving process identity, path traversal, arbitrary code execution, credential exposure, unsafe CDP binding, or destructive restore behavior. Use GitHub's private security advisory workflow for this repository.

请勿为进程身份、路径穿越、任意代码执行、凭据泄露、不安全 CDP 绑定或破坏性恢复行为创建公开 issue。请使用本仓库 GitHub Private Security Advisory 私下报告。

Include:

- affected version and commit;
- Windows and Codex package versions;
- minimal reproduction;
- expected and actual behavior;
- security impact;
- sanitized logs or proof of concept.

Never include real tokens, API keys, cookies, conversations, or unrelated personal files.

## Security invariants

- CDP binds to loopback only.
- Only a verified registered `OpenAI.Codex` Store package is managed.
- Watcher termination requires executable, command-line token, port, Browser ID, and start-time identity.
- Theme image paths remain within the selected theme directory.
- Theme import rejects reparse points.
- Runtime/config/state replacement is atomic and guarded by path ownership.
- Restarting an existing normal Codex session requires explicit user consent.
- Canceling consent must not change Codex or watcher state.
- The project never requests or stores OpenAI credentials.

## Local CDP exposure

Loopback binding prevents LAN access but does not isolate other processes running as the same Windows user. Treat an active CDP session as a local debugging capability. Avoid untrusted programs, and restore official Codex when the theme is not needed.

## Release integrity

Releases include `SHA256SUMS.txt`. Verify the archive before extraction. The project does not currently provide an Authenticode signature, so Windows may display an unknown-publisher warning. SHA-256 proves byte identity with the GitHub Release asset, not publisher identity.
