# Angelina Gravity Field for ChatGPT Codex

面向最新版 Windows ChatGPT/Codex 桌面应用的安洁莉娜主题。视觉语言来自安洁莉娜原皮：珍珠白机能材质、炭黑信息层、信使红强调色和少量青绿色重力术反馈。

本包基于 `Fei-Away/Codex-Dream-Skin` 2026-07-19 最新 `main`（`e776fa6`）制作，适配外部名称已经变为 ChatGPT、内部 Store 包仍为 `OpenAI.Codex`、主程序为 `app\ChatGPT.exe` 的合并版应用。

主题通过仅监听本机回环地址的 Chromium DevTools Protocol 加载 CSS 和装饰 DOM，不修改 WindowsApps、`app.asar`、官方签名、账号、任务、插件或 API 配置。原生侧栏、项目选择、消息与输入框仍可交互。

## 环境要求

- Microsoft Store 安装的官方 `OpenAI.Codex` 应用
- Node.js 22 或更高版本
- Windows PowerShell 5.1 或更高版本

## 安装

1. 完全退出 ChatGPT/Codex，包括所有窗口。
2. 双击 `Install Angelina Gravity Field.cmd`。
3. 安装完成后，从桌面打开 `Codex Angelina Gravity Field`。
4. 如系统提示重启应用，确认后等待皮肤加载。

安装器不需要管理员权限。它会把经过校验的运行副本安装到 `%LOCALAPPDATA%\CodexDreamSkin\engine`，并创建启动、托盘和恢复快捷方式。安装结束后，当前解压目录可以移动或删除。

也可以从 PowerShell 运行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\install-dream-skin.ps1
```

## 使用

`Codex Angelina Gravity Field - Tray` 提供以下操作：

- 应用或重新应用皮肤
- 暂停或继续显示皮肤
- 更换背景图
- 保存当前主题
- 在亮色 Angelina、Angelina Midnight Gravity、桥本有菜、Gothic Void Crusade 和自建主题之间切换
- 打开图片目录或完整恢复 Codex

Angelina 使用两张独立图片：

- 首页：`assets\angelina-hero.png`，2048 x 1152 PNG
- 任务页：`assets\angelina-thread-bg.jpg`，1280 x 720 预模糊 JPEG

暗色预设 `Angelina Midnight Gravity` 位于 `presets\preset-angelina-midnight-gravity\`：

- 首页：`background.png`，2048 x 1152 黑夜重力场主视觉
- 任务页：`task-background.jpg`，1280 x 720 低干扰夜景背景

从 `3.1.2-angelina` 起，亮色任务顶栏保持完全透明，左侧栏、右侧面板、底部面板以及任务操作、项目操作、个人资料、模型/权限选择等页面内二级菜单均使用可透出连续壁纸的磨砂玻璃；菜单视觉直接沿用聊天气泡的半透明底、细边框和柔和阴影。Midnight 预设使用独立黑夜场景和暗色 token；终端渲染区域仍保留原生背景和完整交互。顶部 `文件/编辑/视图/帮助` 及其 Electron 原生菜单保持不变。

从 `3.1.3-angelina` 起，右上角“置顶摘要”线程浮动卡片也使用同一组气泡磨砂变量，并为标题、条目、禁用态、悬停/焦点和分隔线提供亮暗模式对比处理。

从 `3.1.4-angelina` 起，`request_user_input` 生成的“回答问题”多题选择卡通过官方 `data-codex-composer-request-navigation` 锚点与输入框共用磨砂表面、边框、阴影和 16px 模糊；问题、选项、图标、跳过/提交按钮统一使用白色高对比文字，同时保留单选、自由输入与键盘导航。

双背景会跟随 Angelina 主题保存和切换。通过托盘单独导入一张新图片时，会回到上游兼容的单图自适应模式，不会继续叠加旧的 Angelina 任务背景。

## 更新

退出托盘并完全关闭 ChatGPT/Codex，然后用新包重新运行安装器。受管运行时会原子替换；当前主题、已保存主题和导入图片不会被删除。

## 验证

启动皮肤后运行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-dream-skin.ps1 `
  -ScreenshotPath "$env:TEMP\angelina-gravity-field-check.png"
```

验证会检查 Store 包身份、CDP 回环绑定、皮肤版本、原生侧栏和输入框、装饰层鼠标穿透以及首页结构。截图仍需人工检查文字对比度、裁切和控件交互。

## 恢复与卸载

双击 `Restore Official ChatGPT Codex.cmd`，或使用桌面的 `Codex Angelina Gravity Field - Restore`。

完全删除快捷方式：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\restore-dream-skin.ps1 `
  -RestoreBaseTheme -PromptRestart -Uninstall
```

恢复会关闭已验证的调试会话并重新以普通方式启动官方应用。不会删除你的对话、项目或 Codex 配置中的非外观设置。

## 文件位置

| 用途 | 路径 |
|---|---|
| 受管运行时 | `%LOCALAPPDATA%\CodexDreamSkin\engine` |
| 当前主题 | `%LOCALAPPDATA%\CodexDreamSkin\active-theme` |
| 已保存主题 | `%LOCALAPPDATA%\CodexDreamSkin\themes` |
| 导入图片 | `%LOCALAPPDATA%\CodexDreamSkin\images` |
| 状态和日志 | `%LOCALAPPDATA%\CodexDreamSkin` |
| Codex 配置 | `%USERPROFILE%\.codex\config.toml` |

## 安全边界

- CDP 只绑定 `127.0.0.1`，但同一 Windows 用户下的其他本机进程仍可能访问该调试端口。皮肤运行时不要启动来路不明的软件。
- 脚本只接受已注册、Store 签名且非开发模式的 `OpenAI.Codex` 包。
- 启动通过包清单中的 AppUserModelId 完成，不直接执行受 WindowsApps ACL 限制的路径。
- 配置使用严格 UTF-8、并发变更检测和同目录原子替换。
- 不使用皮肤时执行恢复，关闭调试会话。

本主题是个人非商业同人定制，与 OpenAI、Hypergryph 或 Arknights 官方无关联。公开分发或商用前请自行确认角色和素材权利。底层框架归属见 `LICENSE-UPSTREAM.txt`，素材记录见 `references\asset-provenance.md`。
