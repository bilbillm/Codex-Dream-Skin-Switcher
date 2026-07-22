# 一句话告诉 AI 安装这个项目

> 请为我安装 [bilbillm/Codex-Dream-Skin-Switcher](https://github.com/bilbillm/Codex-Dream-Skin-Switcher)：从 [最新 Release](https://github.com/bilbillm/Codex-Dream-Skin-Switcher/releases/latest) 下载 Windows x64 ZIP，核对 `SHA256SUMS.txt`，解压到一个长期保留且可写的目录，运行根目录的 `安装快捷方式.ps1`，然后通过桌面或开始菜单里的“Codex 自定义主题”启动，并确认热切换服务、当前主题和 Codex 页面注入都正常。

[English](README.en.md) | 简体中文

# Codex Dream Skin Switcher

Windows 版 Codex 自定义主题预览、导入、热切换与一键启动工具。当前 `v0.2.0` 把主题选择、应用和启动 Codex 合并为一个安静、可检查的 Windows 控制台。

[![Release](https://img.shields.io/github/v/release/bilbillm/Codex-Dream-Skin-Switcher?display_name=tag)](https://github.com/bilbillm/Codex-Dream-Skin-Switcher/releases/latest)
[![License: MIT](https://img.shields.io/badge/code%20license-MIT-green.svg)](LICENSE)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4)
![Runtime](https://img.shields.io/badge/Dream%20Skin-3.1.4-0078D4)

> [!IMPORTANT]
> 这不是 Codex 官方主题，也不会注册到官方“设置”主题列表。它通过仅监听本机回环地址的 Chromium DevTools Protocol（CDP）向正在运行的 Codex 渲染页面注入 CSS 和少量装饰 DOM。项目不修改 `WindowsApps`、`app.asar`、官方签名、账号、对话、项目或 API 配置。

> 此 Release 不再内置主题。先导入一个本地主题包；维护的 Angelina 浅色/深色主题在 [Codex Angelina Themes](https://github.com/bilbillm/Codex-Angelina-Themes)。主题制作、启动器机制、验证与发布清单见 [主题制作与维护指南.md](主题制作与维护指南.md)。

## 界面预览

`v0.2.0` 统一控制台将主题预览、主题卡、应用主题和“启动 Codex”按钮放在同一窗口；启动成功后可最小化到系统托盘。

### 置顶摘要磨砂卡片

![置顶摘要磨砂卡片](docs/images/pinned-summary-glass.png)

## 它解决什么问题

Dream Skin 原始运行时非常适合脚本化安装和托盘操作，但维护多个主题时仍有几个日常问题：

- 不打开 JSON 很难直观看到哪个主题是白天、哪个是黑夜。
- 切换前无法同时比较首页图和任务页图。
- 新主题目录容易放错位置，图片路径错误通常要到注入阶段才暴露。
- 普通 Codex、带 CDP 的 Codex 和 watcher 状态容易混淆。
- 从脚本、桌面快捷方式和开始菜单进入时，体验不一致。

本项目在不破坏 Dream Skin 安全边界的前提下增加一个 GUI 层：

- 自动扫描 `themes/<主题>/theme.json`。
- 显示首页和任务页图片预览。
- 明确标记白天、黑夜或自动外观。
- 一键热应用，无需为了换主题重启 Codex。
- 从任意合法主题目录导入，并防止路径穿越与链接文件。
- 一键启动或唤醒带当前主题的 Codex。
- 自动创建桌面和开始菜单快捷方式。

## 核心功能

| 功能 | 行为 |
|---|---|
| 主题目录 | 扫描程序目录下的 `themes`，每个子目录对应一个主题 |
| 图片预览 | 在 GUI 中切换首页图和任务页图，不锁定原文件 |
| 应用主题 | 已连接时即时热切换；未连接时安全保存为下次启动主题 |
| 启动 Codex | 先保存所选主题；健康会话直接唤醒，缺失时启动 Codex、CDP 和 watcher |
| 主题导入 | 拷贝合法目录，拒绝绝对图片路径、越界路径、reparse point 和危险目录名 |
| 状态显示 | 显示 watcher 是否连接、当前主题和操作结果 |
| 快捷方式 | 安装一个无参数的桌面/开始菜单控制台入口 |
| 可恢复性 | 不改官方安装文件；关闭调试会话或使用引擎恢复脚本即可回到官方外观 |

## 主题包

启动器只分发 GUI、受管运行时和可信 renderer adapter。主题包独立分发、由用户导入，不能携带可执行脚本。

- [Codex Angelina Themes](https://github.com/bilbillm/Codex-Angelina-Themes) 提供 Angelina Gravity Field 浅色主题与 Angelina Midnight Gravity 深色主题。
- 在控制台中选择“导入主题”，选择主题目录顶层的 `theme.json`；也可以把主题目录复制到程序旁的 `themes/` 后刷新目录。
- `variant: "angelina"` 仍由启动器内的可信 adapter 支持，但图片、主题 JSON、素材来源和视觉维护已迁移到主题仓库。

## 系统要求

### 使用 Release

- Windows 10 或 Windows 11 x64。
- Microsoft Store 安装的官方 Codex/ChatGPT 桌面应用，内部包名为 `OpenAI.Codex`。
- Node.js 22 或更高版本；已验证版本为 `24.7.0`。
- Windows PowerShell 5.1（Windows 自带）。
- Release 中的 GUI 是自包含程序，无需另装 .NET Desktop Runtime。
- `v0.2.0` 自包含包固定使用 .NET Windows Desktop Runtime `10.0.10`。

### 从源码构建

- .NET SDK 10.0。
- PowerShell 7 可用于开发命令，但 Dream Skin 启动脚本由 GUI 固定交给 Windows PowerShell 5.1，以避免 PowerShell 7 对 ISO 时间字段自动转成 `DateTime` 后造成严格进程身份比较误判。
- Node.js 22+。
- Git。

> [!NOTE]
> 当前验证目标为 `OpenAI.Codex_26.715.7063.0_x64__2p2nqsd0c76g0`。Codex 桌面端更新后，包版本变化是正常的；引擎会动态发现已注册的官方 Store 包，不把这个版本号硬编码为启动目标。

## 安装

### 方法一：让 AI 安装

把 README 开头的那句话直接发给一个能访问本机文件、PowerShell 和 GitHub 的 AI 编程代理。代理至少应完成以下核对：

1. 从 `releases/latest` 下载 ZIP 和 `SHA256SUMS.txt`。
2. 对 ZIP 计算 SHA-256，并与清单比较。
3. 解压到长期保留的目录，例如 `D:\codex自定义主题` 或 `%LOCALAPPDATA%\CodexDreamSkinSwitcher`。
4. 运行：

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\安装快捷方式.ps1
   ```

5. 启动“Codex 自定义主题”。
6. 确认 `%LOCALAPPDATA%\CodexDreamSkin\state.json` 中的 watcher 存活，且活动页面含 `codex-dream-skin` 类。

### 方法二：手动安装

1. 打开 [Releases](https://github.com/bilbillm/Codex-Dream-Skin-Switcher/releases/latest)。
2. 下载：
   - `Codex-Dream-Skin-Switcher-v0.2.0-win-x64.zip`
   - `SHA256SUMS.txt`
3. 在 PowerShell 中校验：

   ```powershell
   Get-FileHash -Algorithm SHA256 .\Codex-Dream-Skin-Switcher-v0.2.0-win-x64.zip
   Get-Content .\SHA256SUMS.txt
   ```

4. 把 ZIP 解压到不会随手删除的位置。程序依赖同目录的 `engine`、`themes` 和 `switch-theme.ps1`，不要只把 EXE 单独移走。
5. 右键 `安装快捷方式.ps1`，选择“使用 PowerShell 运行”；或者在终端执行：

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\安装快捷方式.ps1
   ```

6. 从桌面或开始菜单打开“Codex 自定义主题”。

### 安装脚本会做什么

默认创建两个 `.lnk` 文件：

| 位置 | 名称 | 参数 | 用途 |
|---|---|---|---|
| 桌面 | `Codex 自定义主题` | 无 | 打开统一控制台 |
| 开始菜单 | `Codex 自定义主题` | 无 | 与桌面入口相同 |

安装器不会：

- 复制或移动程序目录。
- 请求管理员权限。
- 修改 WindowsApps。
- 修改 Codex 账号或 API 密钥。
- 自动固定到任务栏。
- 删除已有 Dream Skin 快捷方式。

安装器会清理本项目旧版的“Codex 主题切换器”开始菜单入口。

可选参数：

```powershell
# 只创建开始菜单项
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\安装快捷方式.ps1 -SkipDesktop

# 只创建桌面项
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\安装快捷方式.ps1 -SkipStartMenu
```

## 日常使用

### 选择、应用和启动

1. 打开“Codex 自定义主题”，在主题卡中选择一个主题并比较首页/任务页预览。
2. 点击“应用主题”：已连接的 Codex 会立即热切换；未连接时，所选主题会保存为下次启动主题。
3. 点击“启动 Codex”：控制台先安全保存所选主题，再验证健康会话或启动 Codex、CDP 和 watcher。
4. 启动成功后，控制台默认最小化到系统托盘；双击托盘图标可恢复。设置中可改为保持窗口打开。

首次遇到普通 Codex 会话时，运行时会在重启前询问，以避免丢失未发送输入。健康会话唤醒约需 1.2 秒；watcher 停止后的完整恢复约需 18 秒。

### 导入主题

点击“导入主题”，选择一个直接包含 `theme.json` 的文件夹。导入器会：

- 验证必填字段。
- 验证首页图和可选任务页图存在。
- 要求图片路径为相对路径。
- 确保解析后的图片仍在主题目录内。
- 拒绝目录链接、文件链接和 reparse point。
- 清理 Windows 非法文件名、`.`、`..` 和设备保留名。
- 同 ID 已存在时询问是否更新。

## 主题格式

最小主题：

```json
{
  "schemaVersion": 1,
  "id": "my-theme",
  "name": "My Theme",
  "image": "background.png",
  "appearance": "dark"
}
```

完整示例：

```json
{
  "schemaVersion": 1,
  "id": "my-theme",
  "name": "My Theme",
  "skinRevision": "3.1.4-angelina",
  "visualRevision": 1,
  "variant": "angelina",
  "brandSubtitle": "CUSTOM / THEME",
  "tagline": "A short line shown on the home route.",
  "projectPrefix": "PROJECT / ",
  "projectLabel": "SELECT PROJECT",
  "statusText": "SYSTEM · READY",
  "quote": "Optional quote.",
  "image": "background.png",
  "taskImage": "task-background.jpg",
  "appearance": "dark",
  "art": {
    "focusX": 0.72,
    "focusY": 0.42,
    "safeArea": "left",
    "taskMode": "ambient"
  },
  "palette": {
    "accent": "#c85b55"
  }
}
```

字段说明见 [`docs/THEME-FORMAT.md`](docs/THEME-FORMAT.md)。最重要的规则：

- `id` 在主题库中应保持唯一和稳定。
- `image` 与 `taskImage` 必须是相对路径。
- `appearance` 使用 `light`、`dark` 或 `auto`。
- 不要把脚本、HTML 或远程 URL 放进图片字段。
- 推荐首页 16:9、任务页 16:9；任务页图片应降低对比度和细节密度。

## 工作原理

```mermaid
flowchart LR
    A["Codex自定义主题.exe"] -->|"读取"| B["themes/*/theme.json"]
    A -->|"应用"| C["switch-theme.ps1"]
    C -->|"校验、-SaveOnly 并复制"| D["%LOCALAPPDATA%/CodexDreamSkin/active-theme"]
    A -->|"启动 Codex"| G["start-dream-skin.ps1"]
    A -->|"连接时热切换"| E["127.0.0.1 CDP"]
    G --> H["Codex + watcher"]
    H --> E
    E --> I["Codex renderer: CSS + decorative DOM"]
```

模块边界：

| 模块 | 责任 |
|---|---|
| `src/CodexThemeSwitcher` | 统一控制台、目录扫描、预览、导入、启动、托盘和进程调用 |
| `switch-theme.ps1` | 主题路径边界、Dream Skin 主题校验、离线活动主题写入和一次性热应用 |
| `engine/scripts/common-windows.ps1` | 官方 Store 包发现、进程身份、端口和原子文件操作 |
| `engine/scripts/theme-windows.ps1` | 主题存储、主题切换、暂停状态和操作 UI |
| `engine/scripts/injector.mjs` | CDP 会话、目标发现、注入、watch 和 verify |
| `engine/assets/renderer-inject.js` | 渲染器内 CSS/DOM 安装、观察和清理 |
| `engine/assets/dream-skin.css` | 主题视觉、磨砂玻璃和布局覆盖 |

## 文件与运行时位置

| 内容 | 默认位置 |
|---|---|
| 程序与内置主题 | 你解压 Release 的目录 |
| 当前活动主题 | `%LOCALAPPDATA%\CodexDreamSkin\active-theme` |
| 已保存 Dream Skin 主题 | `%LOCALAPPDATA%\CodexDreamSkin\themes` |
| 导入图片 | `%LOCALAPPDATA%\CodexDreamSkin\images` |
| watcher state | `%LOCALAPPDATA%\CodexDreamSkin\state.json` |
| watcher 日志 | `%LOCALAPPDATA%\CodexDreamSkin\injector.log` |
| watcher 错误日志 | `%LOCALAPPDATA%\CodexDreamSkin\injector-error.log` |
| verify 结果 | `%LOCALAPPDATA%\CodexDreamSkin\verify.log` |

## 安全与隐私

### 项目不会读取或上传的内容

- OpenAI API 密钥或 Codex 登录令牌。
- 对话内容和项目文件。
- 浏览器 Cookie。
- 其他应用数据。

### CDP 风险边界

Dream Skin 需要给 Codex 启用 Chromium 调试端口。引擎执行以下限制：

- 地址固定为 `127.0.0.1`，不监听局域网接口。
- 端口必须在有效范围内，默认 `9335`。
- 使用 Store 注册信息验证 Codex 包与可执行文件。
- state 记录 Browser ID、Codex 包、Node 路径、PID 和启动时间。
- 停止 watcher 前重新验证 Node 路径、命令行、端口、Browser ID 与启动时间，避免误杀 PID 复用后的无关进程。

即使只绑定回环地址，同一 Windows 用户下的其他本机进程仍可能访问调试端口。Dream Skin 运行期间不要启动来历不明的软件；不使用主题时可执行恢复脚本关闭调试会话。

### 主题导入边界

GUI 把主题视为数据，而不是代码。它不执行主题目录中的脚本；只读取 JSON 和图片。路径解析使用完整路径规范化，不允许图片越出主题目录。

更多说明见 [`SECURITY.md`](SECURITY.md)。

## 更新

1. 完全退出控制台（如已最小化到托盘，请在托盘菜单选择“退出控制台”）。
2. 下载新 Release 并校验 SHA-256。
3. 解压到新目录，或覆盖旧程序目录中的静态文件。
4. 再次运行 `安装快捷方式.ps1`，让快捷方式指向新目录。
5. 打开控制台，选择主题并检查 watcher 状态。

`%LOCALAPPDATA%\CodexDreamSkin` 中的当前主题、已保存主题和导入图片不会因为替换程序目录自动删除。

## 卸载与恢复

### 仅删除本项目快捷方式

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\卸载快捷方式.ps1
```

该脚本删除本项目创建的两个当前快捷方式及旧版切换器入口，不删除程序文件或 Dream Skin 状态。

### 恢复官方 Codex 外观

在程序目录运行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\engine\scripts\restore-dream-skin.ps1 `
  -RestoreBaseTheme -PromptRestart
```

如需同时卸载 Dream Skin 管理目录和旧快捷方式，请先阅读脚本参数，再使用 `-Uninstall`。恢复操作不会删除对话、项目或非外观 Codex 配置。

## 故障排查

| 症状 | 可能原因 | 处理 |
|---|---|---|
| 显示“等待启动 Codex” | Codex 尚未通过 Dream Skin 连接，或 watcher 已退出 | 在控制台选择主题后点击“启动 Codex” |
| 应用主题未即时更新 | CDP 不可用或 Browser ID 已变化 | 主题已保存为下次启动主题；点击“启动 Codex”恢复连接 |
| 控制台长时间显示进度 | Codex 启动慢、Store 更新中或 verify 超时 | 等待 45 秒；检查 `injector-error.log` 和 `verify.log` |
| 首次启动询问重启 | 当前 Codex 是普通会话，没有 CDP | 保存未发送文本后确认重启；取消不会强制关闭 Codex |
| 主题未出现在列表 | 缺少 `theme.json`、字段非法或图片不存在 | 对照主题格式，确保图片路径相对且文件存在 |
| 图片可应用但 GUI 无法预览 | WinForms 不支持该图片编码 | 转成标准 RGB PNG/JPEG；Dream Skin 仍可能支持原文件 |
| 页面更新后局部样式失效 | Codex DOM 或 class 名变化 | 记录 Codex 版本与截图，提交 issue；不要修改 WindowsApps |
| 端口 9335 被占用 | 其他程序监听该端口 | 关闭未知监听程序；不要连接未经验证的端口 |
| PowerShell 执行策略阻止脚本 | 系统策略较严格 | 使用文档中的 `-ExecutionPolicy Bypass -File` 方式；不要全局降低策略 |
| 开始菜单搜不到快捷方式 | Windows 索引尚未刷新 | 直接打开开始菜单 Programs 目录，或重新运行安装脚本 |

更完整的诊断步骤见 [`docs/TROUBLESHOOTING.md`](docs/TROUBLESHOOTING.md)。报告问题时请附：

- Windows 版本。
- Codex Store 包版本。
- 项目 Release 版本。
- `state.json` 中除 Browser ID 外的结构信息。
- `injector-error.log` 和 `verify.log`。
- 去除账号、路径隐私后的截图。

不要提交 API 密钥、认证文件、对话内容或完整用户目录清单。

## 从源码开发

```powershell
git clone git@github.com:bilbillm/Codex-Dream-Skin-Switcher.git
cd Codex-Dream-Skin-Switcher
dotnet build .\src\CodexThemeSwitcher\CodexThemeSwitcher.csproj -c Release
```

运行开发版前，开发输出目录必须能找到 `engine`、`themes` 和 `switch-theme.ps1`。最简单的完整验证方式是生成 Release 包：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\build-release.ps1 -Version 0.2.0

powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\verify-release.ps1 -Version 0.2.0
```

产物：

```text
artifacts/
├─ Codex-Dream-Skin-Switcher-v0.2.0-win-x64/
├─ Codex-Dream-Skin-Switcher-v0.2.0-win-x64.zip
├─ publish/
└─ SHA256SUMS.txt
```

若只想生成依赖本机 .NET Desktop Runtime 的小体积版本：

```powershell
.\scripts\build-release.ps1 -Version 0.2.0 -FrameworkDependent
```

如需在未来版本显式选择其他 runtime patch，可传入 `-RuntimeFrameworkVersion`；发布者必须确保对应 win-x64 runtime pack 可从 NuGet 或本机缓存恢复。

维护者在完成测试和构建后，可使用已认证的 GitHub CLI 会话发布：

```powershell
.\scripts\publish-release.ps1 -Version 0.2.0 -ReleaseTitle 'v0.2.0 - 统一启动控制台 / Unified launch console'
```

脚本通过 SSH 推送 Git，并通过 `gh` 创建公开仓库、设置 topics 和上传 Release 资产；它不会读取或输出明文令牌。

## 测试

GUI 项目：

```powershell
dotnet build .\src\CodexThemeSwitcher\CodexThemeSwitcher.csproj -c Release
```

Dream Skin JavaScript 测试：

```powershell
node --test .\engine\tests\*.test.mjs
```

Dream Skin Windows 综合测试：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\engine\tests\run-tests.ps1
```

发布包验证：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-release.ps1
```

`v0.2.0` 发布前验证包括：

- .NET Release 构建 0 警告、0 错误。
- 空主题目录、离线导入和已选主题保存校验。
- GUI `--self-test`。
- 主题白天→黑夜→白天真实热切换。
- 页面 `codex-dream-skin`、`dream-theme-light` 和 `dream-theme-dark` 标记。
- watcher 缺失时的冷启动。
- 健康 watcher 的快速唤醒。
- 离线主题保存、托盘恢复和 150% DPI 下统一控制台截图检查。
- 发布 ZIP 清单、SHA-256 和敏感凭据模式扫描。

## 项目结构

```text
Codex-Dream-Skin-Switcher/
├─ src/CodexThemeSwitcher/  # WinForms 源码和热切换桥接脚本
├─ engine/                   # Dream Skin 运行时、可信 renderer adapter 与测试
├─ themes/                   # 外部主题导入目录，不含内置主题
├─ scripts/                  # 构建、发布、快捷方式安装和验证
├─ docs/                     # 主题格式、架构、排障和截图
├─ 主题制作与维护指南.md       # 主题包、启动器、验证与发布规范
├─ README.md                 # 中文主文档
├─ README.en.md              # 英文文档
├─ CHANGELOG.md
├─ SECURITY.md
├─ THIRD-PARTY-NOTICES.md
└─ LICENSE
```

## 已知限制

- 仅支持 Windows x64。
- 不是官方主题注册机制，无法出现在 Codex 官方设置主题列表。
- 顶部 Electron 原生菜单不应用磨砂玻璃。
- Codex 大版本更新可能改变 DOM，需要更新选择器。
- 启用 CDP 会扩大同一用户本机进程的调试能力边界。
- 主题导入只复制主题数据，不自动安装额外字体、程序或远程资源。
- 当前内置角色主题涉及第三方角色权利，不建议未经确认用于商业分发。

## FAQ

### 为什么不用修改 `app.asar`？

修改 `app.asar` 或 WindowsApps 内容会破坏签名、更新和恢复路径。CDP 注入不改官方包，关闭会话即可清除渲染器内状态。

### 为什么切换主题不需要重启？

watcher 保持已验证的 CDP 连接。切换器更新活动主题后执行一次性注入，渲染器替换 CSS 变量、图片 URL 和主题 class。

### 启动和主题切换在同一个界面吗？

是。Release 只有一个 `Codex自定义主题.exe`，双击后始终打开统一控制台。选择主题不会自动改变 Codex；点击“应用主题”或“启动 Codex”才会生效。旧版 `--launch` 参数仅为旧快捷方式兼容保留，同样打开控制台。

### 可以把主题放在别的盘吗？

程序可以放在任意可写的长期目录，但可扫描主题必须位于它旁边的 `themes` 目录。GUI 的“导入主题”会把外部主题复制进来。

### 可以只用自己的图片吗？

可以。复制一个现有主题目录，修改 `id`、`name`、`image`、`taskImage` 和元数据，然后刷新主题库。不要覆盖内置主题的稳定 ID。

### 会影响 Codex 自动更新吗？

不会修改 Store 包。更新后可重新打开控制台并点击“启动 Codex”；若 DOM 变化导致样式失效，请升级本项目。

### 为什么启动脚本使用 Windows PowerShell 5.1？

Dream Skin state 需要对 ISO 时间字符串做精确进程身份比较。PowerShell 7 的 `ConvertFrom-Json` 会自动产生 `DateTime`，字符串化后受地区格式影响；5.1 保持原始字符串，符合引擎的身份模型。

## 贡献

欢迎提交：

- 不含版权争议资源的原创主题。
- 新 Codex 版本的选择器兼容修复。
- 可复现的启动、热切换或 DPI 问题。
- 中英文文档修订。
- 安全边界改进和测试。

提交代码前请阅读 [`CONTRIBUTING.md`](CONTRIBUTING.md)。主题贡献必须说明素材来源和再分发权限。

## 上游与致谢

- Dream Skin 上游：[Fei-Away/Codex-Dream-Skin](https://github.com/Fei-Away/Codex-Dream-Skin)
- 本包基于上游提交：`e776fa6d5361a2bdd5c1614674397681e7b00874`
- Dream Skin 运行时版本：`3.1.4-angelina`
- 详细构建信息：[`engine/BUILD-INFO.md`](engine/BUILD-INFO.md)
- 素材来源：[`engine/references/asset-provenance.md`](engine/references/asset-provenance.md)

## 许可证与非隶属声明

本项目源代码使用 [MIT License](LICENSE)。内置 Dream Skin 上游许可证见 [`engine/LICENSE-UPSTREAM.txt`](engine/LICENSE-UPSTREAM.txt)。

主题图片、第三方角色、名称、商标和参考作品不因代码 MIT 许可证而获得重新授权。请阅读 [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)。本项目与 OpenAI、鹰角网络、悠星或《明日方舟》官方无隶属、授权或赞助关系。
