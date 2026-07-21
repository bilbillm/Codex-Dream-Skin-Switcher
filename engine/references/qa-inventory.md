# QA inventory

## Angelina 3.1.2 additions

- 首页主题必须显示 `angelina-hero.png`，信息标识层贴合原生主视觉边界且不拦截鼠标。
- 普通任务页必须使用 `angelina-thread-bg.jpg`，消息、代码和 composer 本身不模糊。
- 收起与展开左侧栏时皮肤保持，不闪回原生配色。
- 托盘切换到桥本有菜或 Gothic Void Crusade 后不得残留 Angelina 专属类、信息标识或任务背景。
- 从已保存主题切回 Angelina 时，首页图、任务图和 `variant=angelina` 必须同时恢复。
- 左侧栏必须透出连续壁纸并保持文字可读；新建任务/对话、项目操作、项目内新建任务、置顶与归档图标使用同一信使红动作色。
- 右侧面板与底部面板的外壳、标签栏和工具栏必须使用对应的亮/暗磨砂变量；原生分隔条、拖拽、缩放和标签交互不得改变。
- 底部与右侧终端的 `data-codex-terminal` 渲染区域必须保留原生终端背景、文字对比度和输入交互。
- Radix menu/listbox/popover/select 与同类页面内二级菜单必须使用聊天气泡同系的亮/暗磨砂变量，菜单项点击、键盘焦点、禁用态和快捷键不得改变。
- 顶部 `文件/编辑/视图/帮助` 及其 Electron 原生菜单不得被替换或拦截。
- 亮色任务顶栏必须保持透明，同时维持固定定位；首页至少有两个、任务页至少有三个可见的原生面板控件。
- 编辑消息表单必须与普通用户消息气泡同色；加号弹层、进度胶囊、目标与排队区域必须与输入框同系且使用浅色文字。
- `Angelina Midnight Gravity` 必须使用独立夜景双背景、`appearance=dark` 和 Angelina 暗色变量；不得只是亮色图叠加黑色滤镜。
- 1280 x 820 和窄窗口下不应出现横向滚动；窄窗口会隐藏状态、坐标与引语装饰。

## User-visible claims

1. The home screen paints one UI-free wallpaper continuously across sidebar and main content, with a live native heading, the real project utility/composer surface, and any suggestion cards rendered by the current Codex host.
2. Sidebar, main area, header, and composer use coordinated readability layers; home remains expressive while normal task routes use a stronger quiet veil.
3. All real Codex controls remain interactive; the skin is not a screenshot overlay.
4. The skin survives route changes and renderer reloads while the injector daemon runs.
5. The official Store package and `app.asar` remain unchanged.
6. Restore removes the injected DOM/CSS and install/restore can be repeated.
7. Restore closes the saved CDP listener before reopening Codex normally.

## Functional checks

- Home feature card: click one card and confirm the real composer is populated or the normal action occurs.
- Project selector: click the real project chip under the "选择项目" label and confirm the native project menu opens.
- Sidebar: open a real task, then return to New Task.
- Task side panel: open and close the native thread panel twice, resize the window, and repeat; the toggle must remain visible and clickable.
- Composer: type text, verify caret/readability, then clear it without sending.
- Reload: use CDP `Page.reload`, wait, and confirm the injection marker returns.
- Pet overlay: open a desktop pet and confirm its auxiliary window stays transparent with no skin background or decoration layer behind it.
- Restore/reapply cycle: remove live skin, verify marker absent, apply again, verify marker present.
- Update resilience: resolve the current `OpenAI.Codex` Appx location dynamically for launch. A versioned path saved for cleanup must be revalidated against the registered package full/family identity before any process is stopped.
- Restart consent: an existing normal Codex window is never force-closed without explicit CLI authorization or shortcut confirmation.
- Shortcut policy: installed launch, restore, tray, and tray-child commands use `RemoteSigned` without `Bypass`; Internet-zone markers are removed only from hash-verified managed PowerShell copies.
- Config safety: Chinese project names, LF/CRLF choice, quoted target keys, table-header comments, and unrelated TOML sections survive install/selective restore; ambiguous target shapes fail unchanged, exact recovery keeps a copy of the replaced current file, and install refuses both registered and state-recorded old Codex processes.
- Theme safety: empty/over-16 MB images, over-16384px/50MP dimensions, path escapes, symlinks/junctions, malformed JSON, and unsupported formats are rejected before payload construction.
- Tray lifecycle: pause/resume reflects the clicked state, bundled Arina Hashimoto theme is present on first install, and complete restore terminates any separately launched tray before it can reapply the skin.

## Visual checks

- 1280x820 initial home: the declared focus stays in frame, the text-safe side remains readable, the real project utility row and composer form one coherent surface, and no horizontal scrolling appears.
- Narrower window: accept Codex's native responsive card reduction or omission; no essential control is covered and wallpaper cropping preserves the focus/safe-area contract.
- Normal task: the wallpaper is visibly quieter than home, messages keep high contrast, and composer does not overlap content.
- Inspect the sidebar, header, wallpaper edges, native card labels when present, project utility row, composer controls, scrollbar, dialogs, and menus.
- Reject black/transparent sidebar artifacts, clipped controls, duplicated/disconnected project labels, rasterized native controls, fake UI inside the wallpaper, weak contrast, or decorations intercepting clicks.

## Exploratory checks

- Start when the debug port is occupied: fail with a clear message or use a caller-selected port.
- Start after Codex updates: package discovery and injection still work without patching installed files.
- Tamper `state.json` with a reused PID: if the PID is still live but its identity differs, confirm cleanup fails closed and preserves `state.json`; if the PID is gone, confirm the stale record is replaced only after confirming no process is running, without stopping an unrelated process.
- Serve a fake `app://` CDP target or remote/mismatched WebSocket URL and confirm both launcher and injector reject it. Reuse the port with a new Browser ID and confirm the existing watcher exits without reconnecting.
- Force verification failure and confirm the injector, state file, and newly launched debug session are rolled back.
- Start two operations concurrently and confirm the second fails clearly without changing config, state, or processes.
- Close Codex without restore and confirm the Browser identity anchor closes and the watcher exits without reconnecting or rapidly growing logs.

## Automated checks

- `tests/run-tests.ps1`: strict UTF-8/no-BOM writes, UTF-16 rejection, LF/CRLF preservation, concurrent-write detection, exact backup/recovery, `[desktop]`-scoped restore, ambiguous TOML rejection, non-ASCII paths, Appx/state identity, argument quoting, theme seeding/import/save/switch/pause, byte/dimension limits, junction rejection, payload construction, Browser ID, loopback URL rejection, and renderer isolation for transparent auxiliary windows.
- `node --check` for the injector and renderer payload.
- Live Windows signoff remains required for Store process ownership, restart consent, screenshot, and CDP closure.
