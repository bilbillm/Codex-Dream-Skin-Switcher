# Theme format / 主题格式

[中文](#中文) | [English](#english)

## 中文

每个可切换主题必须位于独立目录，目录顶层包含 `theme.json` 和本地图片：

```text
themes/
└─ my-theme/
   ├─ theme.json
   ├─ background.png
   └─ task-background.jpg
```

### 字段参考

| 字段 | 必填 | 类型 | 含义 |
|---|---:|---|---|
| `schemaVersion` | 建议 | number | 当前格式版本为 `1` |
| `id` | 是 | string | 稳定唯一 ID；导入目录名由它派生 |
| `name` | 是 | string | GUI 显示名称 |
| `image` | 是 | string | 首页图片相对路径 |
| `taskImage` | 否 | string | 任务页图片相对路径；省略时复用首页图 |
| `appearance` | 否 | string | `light`、`dark` 或 `auto`；默认 `auto` |
| `skinRevision` | 否 | string | 目标 Dream Skin 修订，例如 `3.1.4-angelina` |
| `visualRevision` | 否 | number | 主题自身视觉修订号 |
| `variant` | 否 | string | CSS 变体；本仓库使用 `angelina` |
| `brandSubtitle` | 否 | string | 首页品牌副标题 |
| `tagline` | 否 | string | 首页短句 |
| `projectPrefix` | 否 | string | 项目名称前缀 |
| `projectLabel` | 否 | string | 项目选择区标签 |
| `statusText` | 否 | string | 状态行文本 |
| `quote` | 否 | string | 可选引语 |
| `art.focusX` | 否 | number | 主视觉水平焦点，通常在 `0` 到 `1` |
| `art.focusY` | 否 | number | 主视觉垂直焦点，通常在 `0` 到 `1` |
| `art.safeArea` | 否 | string | 文本安全区提示，例如 `left` |
| `art.taskMode` | 否 | string | 任务图策略，例如 `ambient` |
| `palette.accent` | 否 | string | 主题强调色，建议使用十六进制色值 |

### 强制安全规则

- `image` 和 `taskImage` 必须是相对路径。
- 规范化后的路径必须仍位于当前主题目录。
- 图片必须存在并且是普通文件。
- 导入器拒绝 reparse point、符号链接和目录链接。
- `id` 不应为 `.`, `..`、空字符串或 Windows 设备保留名。
- 主题目录中的脚本不会被执行，也不应该作为主题依赖。
- 不支持 `http://`、`https://`、`data:` 或其他远程/内联图片 URL。

### 图片建议

- 首页：16:9，推荐 1920×1080 或 2048×1152。
- 任务页：16:9，推荐 1280×720；降低对比度、饱和度和高频细节。
- 使用标准 RGB PNG 或 JPEG，提高 WinForms 预览兼容性。
- 预留文本安全区，不要让人物面部或高对比主体位于主要文本后方。
- 亮色和暗色主题分别验证正文、次要文字、边框和焦点状态对比度。

### 更新主题

保持 `id` 不变，增加 `visualRevision`，替换图片或元数据，然后在 GUI 中重新导入或直接覆盖主题目录并点击刷新。改变 `id` 会被视为新主题。

## English

Each switchable theme lives in its own directory with a top-level `theme.json` and local images.

### Required fields

- `id`: stable unique identifier.
- `name`: display name.
- `image`: relative path to the home image.

### Optional fields

- `taskImage`: relative task background; home image is reused when omitted.
- `appearance`: `light`, `dark`, or `auto`.
- `skinRevision`, `visualRevision`, and `variant`: compatibility metadata.
- `brandSubtitle`, `tagline`, `projectPrefix`, `projectLabel`, `statusText`, and `quote`: display copy.
- `art.focusX`, `art.focusY`, `art.safeArea`, and `art.taskMode`: composition hints.
- `palette.accent`: semantic accent color.

### Security rules

Image paths must be relative, normalize inside the theme root, and resolve to ordinary existing files. Reparse points and remote/inline URLs are rejected. Theme scripts are never executed.

Use standard RGB PNG/JPEG assets. Prefer 16:9 home art and a lower-detail task background. Keep IDs stable across visual updates and increment `visualRevision`.
