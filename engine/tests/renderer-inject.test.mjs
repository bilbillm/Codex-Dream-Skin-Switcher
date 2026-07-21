import assert from "node:assert/strict";
import fs from "node:fs/promises";
import path from "node:path";
import vm from "node:vm";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const windowsRoot = path.resolve(here, "..");
const template = await fs.readFile(path.join(windowsRoot, "assets", "renderer-inject.js"), "utf8");
const css = await fs.readFile(path.join(windowsRoot, "assets", "dream-skin.css"), "utf8");
const buildPayload = (config = {}, taskArt = "data:image/jpeg;base64,AA==") => template
  .replace("__DREAM_CSS_JSON__", JSON.stringify(".fixture { color: blue; }"))
  .replace("__DREAM_ART_JSON__", JSON.stringify("data:image/png;base64,AA=="))
  .replace("__DREAM_TASK_ART_JSON__", JSON.stringify(taskArt))
  .replace("__DREAM_THEME_JSON__", JSON.stringify(config));
const payload = buildPayload();

const singleImagePayload = buildPayload({}, null);

assert.match(css, /--agl-glass-chrome:\s*rgba\(47, 56, 64, \.44\)/);
assert.match(css, /--agl-glass-composer:\s*rgba\(53, 60, 65, \.58\)/);
assert.match(
  css,
  /dream-variant-angelina main\.main-surface\s*>\s*header\.app-header-tint\s*\{[^}]*position:\s*fixed\s*!important;[^}]*z-index:\s*30\s*!important;[^}]*background:\s*var\(--agl-glass-chrome\)/s,
);
assert.match(
  css,
  /dream-variant-angelina\.dream-theme-light\s*main\.main-surface\s*>\s*header\.app-header-tint\s*\{[^}]*background:\s*transparent\s*!important;[^}]*backdrop-filter:\s*none\s*!important/s,
);
assert.match(
  css,
  /dream-variant-angelina \.composer-surface-chrome\s*\{[^}]*background:\s*var\(--agl-glass-composer\)/s,
);
assert.match(
  css,
  /dream-variant-angelina \.thread-scroll-container\s*\{[^}]*--color-token-conversation-body:\s*var\(--agl-ink\)/s,
);
assert.match(css, /dream-variant-angelina \.dream-task::before\s*\{[^}]*filter:\s*blur\(2px\)/s);
assert.match(
  css,
  /dream-variant-angelina\.dream-art-wide[^}]*aside\.app-shell-left-panel\s*\{[^}]*border-right:\s*1px solid var\(--agl-sidebar-border\)[^}]*background:\s*linear-gradient[^}]*backdrop-filter:\s*blur\(18px\)/s,
);
assert.match(css, /vertical-scroll-fade-mask[^}]*scrollbar-color:\s*var\(--agl-action\) var\(--agl-scrollbar-track\)/s);
assert.match(css, /::-webkit-scrollbar\s*\{[^}]*width:\s*12px/s);
assert.match(
  css,
  /@layer theme\s*\{[^}]*aria-label="置顶任务"[^}]*aria-label="取消置顶任务"[^}]*aria-label="归档任务"[^}]*aria-label\$=" 的项目操作"[^}]*aria-label\$=" 中新建任务"[^}]*button svg[^}]*color:\s*var\(--agl-action\)/s,
);
assert.match(
  css,
  /bg-token-foreground\/5[^}]*max-w-\[77%\][^}]*rounded-2xl[^}]*background:\s*var\(--agl-user-bubble\)/s,
);
assert.match(css, /data-content-search-unit-key\*="\:user"[^}]*form:has\(\[aria-label="编辑消息"\], \[aria-label="Edit message"\]\)[^}]*background:\s*var\(--agl-user-bubble\)/s);
assert.match(css, /data-composer-overlay-floating-ui[^}]*background:\s*var\(--agl-glass-composer\)/s);
assert.match(css, /data-above-composer-portal[^}]*bg-token-input-background\/70[^}]*background:\s*var\(--agl-glass-utility\)/s);
assert.match(
  css,
  /data-codex-composer-request-navigation[^}]*background:\s*var\(--agl-glass-composer\)[^}]*color:\s*var\(--agl-glass-text\)[^}]*backdrop-filter:\s*blur\(16px\)/s,
);
assert.match(css, /data-codex-composer-request-navigation[^}]*role="radio"[^}]*background:\s*rgba\(255, 255, 255, \.035\)/s);
assert.match(css, /data-codex-composer-request-navigation[^}]*aria-checked="true"[^}]*box-shadow:\s*inset 2px 0 0 var\(--agl-coral\)/s);
assert.match(css, /data-codex-composer-request-navigation[^}]*textarea::placeholder[^}]*color:\s*var\(--agl-glass-muted\)/s);
assert.match(css, /dream-variant-angelina\.dream-theme-dark\s*\{[^}]*--agl-ink:\s*#f2f0ed[^}]*--dream-canvas:\s*#080d13[^}]*color-scheme:\s*dark/s);
assert.match(css, /group\/folder-row[^}]*scrollbar-color:\s*transparent transparent[^}]*scrollbar-width:\s*none/s);
assert.match(css, /--agl-panel-surface:\s*rgba\(43, 51, 58, \.58\)/);
assert.match(css, /dream-theme-dark\s*\{[^}]*--agl-panel-surface:\s*rgba\(10, 17, 24, \.72\)/s);
assert.match(
  css,
  /aside\[data-app-shell-focus-area="right-panel"\][^{]*> \[class~="bg-token-main-surface-primary"\]\s*\{[^}]*background:\s*var\(--agl-panel-surface\)[^}]*backdrop-filter:\s*blur\(18px\)/s,
);
assert.match(
  css,
  /data-app-shell-tab-strip-controller="bottom"[^}]*\)\s*\{[^}]*background:\s*var\(--agl-panel-surface\)[^}]*backdrop-filter:\s*blur\(18px\)/s,
);
assert.match(css, /data-app-shell-tabs="true"[^}]*data-app-shell-tab-strip-controller="bottom"[^}]*background:\s*transparent/s);
assert.match(
  css,
  /\[data-codex-terminal="true"\]\[data-codex-xterm="true"\]\s*\{[^}]*background-color:\s*var\(--vscode-terminal-background\)[^}]*color:\s*var\(--vscode-terminal-foreground\)/s,
);
assert.match(css, /--agl-menu-surface:\s*rgba\(64, 72, 78, \.18\)/);
assert.match(css, /dream-theme-dark\s*\{[^}]*--agl-menu-surface:\s*rgba\(91, 108, 119, \.34\)/s);
assert.match(
  css,
  /data-radix-popper-content-wrapper[^}]*data-radix-menu-content[^}]*role="listbox"[^}]*\{[^}]*background:\s*var\(--agl-menu-surface\)[^}]*backdrop-filter:\s*blur\(18px\)/s,
);
assert.match(css, /data-slot="dropdown-menu-content"[^}]*data-slot="popover-content"[^}]*data-slot="select-content"/s);
assert.match(css, /role="menuitem"[^}]*data-highlighted[^}]*background:\s*var\(--agl-menu-hover\)/s);
assert.match(
  css,
  /top-\(--thread-floating-content-top-inset\)[^}]*bg-token-dropdown-background[^}]*rounded-3xl[^}]*\{[^}]*background:\s*var\(--agl-menu-surface\)[^}]*backdrop-filter:\s*blur\(18px\)/s,
);
assert.match(css, /group\/summary-panel-item[^}]*background:\s*var\(--agl-menu-hover\)/s);
assert.match(css, /top-\(--thread-floating-content-top-inset\)[^}]*section::after\s*\{[^}]*background:\s*var\(--agl-menu-border\)/s);
assert.match(css, /rounded-3xl[^}]*header\[class~="bg-token-dropdown-background"\]\s*\{[^}]*background:\s*transparent/s);

function createFixture({
  shellPresent,
  mainPresent = shellPresent,
  sidebarPresent = shellPresent,
  staleSkin = false,
  homePresent = false,
  utilityPresent = false,
  shellAppearance = "dark",
  computedColorScheme = "",
  osAppearance = "light",
  analysisFixture = null,
}) {
  const nodes = new Map();
  const rootClasses = new Set(staleSkin ? ["codex-dream-skin"] : []);
  const rootStyles = new Map(staleSkin ? [["--dream-art", "url(\"blob:stale\")"]] : []);
  const revokedUrls = [];
  const observers = [];
  let objectUrlCount = 0;
  let hasMain = mainPresent;
  let hasSidebar = sidebarPresent;
  let root;

  const queueRootClassMutation = () => {
    for (const observer of observers) {
      if (observer.target !== root || !observer.options?.attributes) continue;
      if (observer.options.attributeFilter && !observer.options.attributeFilter.includes("class")) continue;
      observer.records.push({ type: "attributes", attributeName: "class", target: root });
    }
  };
  const makeClassList = (classes = new Set(), onMutation = () => {}) => ({
    add(...values) {
      let changed = false;
      for (const value of values) {
        if (!classes.has(value)) { classes.add(value); changed = true; }
      }
      if (changed) onMutation();
    },
    remove(...values) {
      let changed = false;
      for (const value of values) changed = classes.delete(value) || changed;
      if (changed) onMutation();
    },
    toggle(value, enabled) {
      const changed = enabled ? !classes.has(value) : classes.has(value);
      if (enabled) classes.add(value);
      else classes.delete(value);
      if (changed) onMutation();
    },
    contains(value) { return classes.has(value); },
  });

  root = {
    className: shellAppearance,
    classList: makeClassList(rootClasses, queueRootClassMutation),
    getAttribute() { return null; },
    style: {
      setProperty(key, value) { rootStyles.set(key, value); },
      removeProperty(key) { rootStyles.delete(key); },
    },
    appendChild(node) {
      node.parentElement = root;
      nodes.set(node.id, node);
    },
  };
  const body = {
    className: "",
    getAttribute() { return null; },
    appendChild(node) {
      node.parentElement = body;
      nodes.set(node.id, node);
    },
  };
  const shellMain = {
    classList: makeClassList(),
    getBoundingClientRect() {
      return { left: 290, top: 36, width: 990, height: 784 };
    },
  };
  const routeClasses = new Set();
  const utilityClasses = new Set();
  const utilityNode = { classList: makeClassList(utilityClasses) };
  const heroNode = {
    getBoundingClientRect() { return { left: 310, top: 92, width: 910, height: 292 }; },
  };
  const routeMain = {
    classList: makeClassList(routeClasses),
    querySelector(selector) {
      if (selector === ":scope > div:first-child > div:first-child > div:first-child" && homePresent) {
        return heroNode;
      }
      return null;
    },
    querySelectorAll(selector) {
      if (selector === '[class*="_homeUtilityBar_"]' && utilityPresent) return [utilityNode];
      return [];
    },
  };
  const staleHome = { classList: makeClassList(new Set(["dream-home"])) };
  const staleShell = { classList: makeClassList(new Set(["dream-home-shell"])) };

  const createElement = (tagName) => {
    if (tagName === "canvas" && analysisFixture) {
      return {
        width: 0,
        height: 0,
        getContext() {
          return {
            drawImage() {},
            getImageData() { return { data: analysisFixture.pixels }; },
          };
        },
      };
    }
    return {
      id: "",
      dataset: {},
      style: {},
      classList: makeClassList(),
      parentElement: null,
      textContent: "",
      innerHTML: "",
      setAttribute() {},
      remove() { nodes.delete(this.id); },
    };
  };
  if (staleSkin) {
    const style = createElement();
    style.id = "codex-dream-skin-style";
    nodes.set(style.id, style);
    const chrome = createElement();
    chrome.id = "codex-dream-skin-chrome";
    nodes.set(chrome.id, chrome);
  }

  const document = {
    documentElement: root,
    head: root,
    body,
    createElement,
    getElementById(id) { return nodes.get(id) ?? null; },
    querySelector(selector) {
      if (selector === "main.main-surface") return hasMain ? shellMain : null;
      if (selector === "main") return hasMain ? shellMain : null;
      if (selector === "aside.app-shell-left-panel") return hasSidebar ? {} : null;
      if (selector === '[role="main"]:has([data-testid="home-icon"])') {
        return hasMain && homePresent ? routeMain : null;
      }
      if (selector === '[role="main"]') return hasMain ? routeMain : null;
      return null;
    },
    querySelectorAll(selector) {
      if (selector === '[role="main"]') return hasMain ? [routeMain] : [];
      if (selector === ".dream-task") return routeClasses.has("dream-task") ? [routeMain] : [];
      if (selector === ".dream-home-utility") {
        return utilityClasses.has("dream-home-utility") ? [utilityNode] : [];
      }
      if (!staleSkin) return [];
      if (selector === ".dream-home") return [staleHome];
      if (selector === ".dream-home-shell") return [staleShell];
      return [];
    },
  };
  const context = {
    window: {
      matchMedia() { return { matches: osAppearance === "dark" }; },
    },
    document,
    MutationObserver: class {
      constructor(callback) {
        this.callback = callback;
        this.records = [];
        this.target = null;
        this.options = null;
        observers.push(this);
      }
      observe(target, options = {}) {
        this.target = target;
        this.options = options;
      }
      disconnect() {
        this.target = null;
        this.records = [];
      }
      takeRecords() {
        const records = this.records;
        this.records = [];
        return records;
      }
    },
    URL: {
      createObjectURL() { objectUrlCount += 1; return `blob:fixture-${objectUrlCount}`; },
      revokeObjectURL(value) { revokedUrls.push(value); },
    },
    Blob,
    Uint8Array,
    atob,
    setInterval: () => 1,
    clearInterval: () => {},
    setTimeout: () => 2,
    clearTimeout: () => {},
    getComputedStyle() { return { colorScheme: computedColorScheme }; },
  };
  if (analysisFixture) {
    context.Image = class {
      naturalWidth = analysisFixture.naturalWidth;
      naturalHeight = analysisFixture.naturalHeight;
      set src(_) { this.onload(); }
    };
  }

  return {
    context,
    nodes,
    observers,
    rootClasses,
    rootStyles,
    revokedUrls,
    routeClasses,
    utilityClasses,
    setShellPresent(value) {
      hasMain = value;
      hasSidebar = value;
    },
    setSidebarPresent(value) { hasSidebar = value; },
    setMainPresent(value) { hasMain = value; },
  };
}

const main = createFixture({ shellPresent: true });
const mainResult = vm.runInNewContext(payload, main.context);
assert.equal(mainResult.installed, true);
assert.equal(main.rootClasses.has("codex-dream-skin"), true);
assert.equal(main.rootStyles.get("--dream-art"), 'url("blob:fixture-1")');
assert.equal(main.rootStyles.get("--dream-task-art"), 'url("blob:fixture-2")');
assert.equal(main.nodes.has("codex-dream-skin-style"), true);
assert.equal(main.nodes.has("codex-dream-skin-chrome"), true);
assert.equal(main.rootClasses.has("dream-theme-dark"), true);
assert.equal(main.rootClasses.has("dream-art-standard"), true);
assert.equal(main.rootClasses.has("dream-task-ambient"), true);
assert.equal(main.routeClasses.has("dream-task"), true);
assert.equal(main.context.window.__CODEX_DREAM_SKIN_STATE__.cleanup(), true);
assert.equal(main.rootClasses.has("codex-dream-skin"), false);
assert.equal(main.rootClasses.has("dream-theme-dark"), false);
assert.equal(main.nodes.has("codex-dream-skin-style"), false);
assert.equal(main.nodes.has("codex-dream-skin-chrome"), false);
assert.deepEqual(main.revokedUrls, ["blob:fixture-1", "blob:fixture-2"]);

const singleImage = createFixture({ shellPresent: true });
vm.runInNewContext(singleImagePayload, singleImage.context);
assert.equal(singleImage.rootStyles.get("--dream-art"), 'url("blob:fixture-1")');
assert.equal(singleImage.rootStyles.get("--dream-task-art"), 'url("blob:fixture-1")');
assert.equal(singleImage.context.window.__CODEX_DREAM_SKIN_STATE__.cleanup(), true);
assert.deepEqual(singleImage.revokedUrls, ["blob:fixture-1"]);

const reinjected = createFixture({ shellPresent: true });
vm.runInNewContext(payload, reinjected.context);
const firstState = reinjected.context.window.__CODEX_DREAM_SKIN_STATE__;
vm.runInNewContext(payload, reinjected.context);
const secondState = reinjected.context.window.__CODEX_DREAM_SKIN_STATE__;
assert.notEqual(secondState.installToken, firstState.installToken);
assert.equal(secondState.artUrl, "blob:fixture-3");
assert.equal(secondState.taskArtUrl, "blob:fixture-4");
assert.equal(reinjected.rootStyles.get("--dream-art"), 'url("blob:fixture-3")');
assert.deepEqual(reinjected.revokedUrls, ["blob:fixture-1", "blob:fixture-2"]);
assert.equal(firstState.cleanup(), false);
assert.equal(secondState.cleanup(), true);

const auxiliary = createFixture({ shellPresent: false, staleSkin: true });
const auxiliaryResult = vm.runInNewContext(payload, auxiliary.context);
assert.equal(auxiliaryResult.installed, true);
assert.equal(auxiliary.rootClasses.has("codex-dream-skin"), false);
assert.equal(auxiliary.rootStyles.has("--dream-art"), false);
assert.equal(auxiliary.nodes.has("codex-dream-skin-style"), false);
assert.equal(auxiliary.nodes.has("codex-dream-skin-chrome"), false);

auxiliary.setShellPresent(true);
auxiliary.context.window.__CODEX_DREAM_SKIN_STATE__.ensure();
assert.equal(auxiliary.rootClasses.has("codex-dream-skin"), true);
assert.equal(auxiliary.nodes.has("codex-dream-skin-style"), true);
assert.equal(auxiliary.nodes.has("codex-dream-skin-chrome"), true);

// Collapsing the left rail removes aside.app-shell-left-panel while the main
// surface remains. The active theme must stay applied instead of flashing the
// native Codex chrome.
const collapsedSidebar = createFixture({
  shellPresent: true,
  mainPresent: true,
  sidebarPresent: false,
  staleSkin: true,
});
const collapsedResult = vm.runInNewContext(payload, collapsedSidebar.context);
assert.equal(collapsedResult.installed, true);
assert.equal(collapsedSidebar.rootClasses.has("codex-dream-skin"), true);
assert.equal(collapsedSidebar.rootStyles.has("--dream-art"), true);
assert.equal(collapsedSidebar.nodes.has("codex-dream-skin-style"), true);
assert.equal(collapsedSidebar.nodes.has("codex-dream-skin-chrome"), true);
assert.equal(collapsedSidebar.rootClasses.has("dream-theme-dark"), true);

collapsedSidebar.setSidebarPresent(false);
collapsedSidebar.context.window.__CODEX_DREAM_SKIN_STATE__.ensure();
assert.equal(collapsedSidebar.rootClasses.has("codex-dream-skin"), true);
assert.equal(collapsedSidebar.nodes.has("codex-dream-skin-style"), true);

collapsedSidebar.setMainPresent(false);
collapsedSidebar.context.window.__CODEX_DREAM_SKIN_STATE__.ensure();
assert.equal(collapsedSidebar.rootClasses.has("codex-dream-skin"), false);
assert.equal(collapsedSidebar.nodes.has("codex-dream-skin-style"), false);

const configured = createFixture({
  shellPresent: true,
  homePresent: true,
  utilityPresent: true,
});
const configuredPayload = buildPayload({
  appearance: "light",
  palette: { accent: "#d45a70" },
  art: { focusX: .15, focusY: .8, safeArea: "right", taskMode: "off" },
});
const configuredResult = vm.runInNewContext(configuredPayload, configured.context);
assert.equal(configuredResult.adaptive, true);
assert.equal(configured.rootClasses.has("dream-theme-light"), true);
assert.equal(configured.rootClasses.has("dream-theme-dark"), false);
assert.equal(configured.rootClasses.has("dream-focus-left"), true);
assert.equal(configured.rootClasses.has("dream-safe-right"), true);
assert.equal(configured.rootClasses.has("dream-task-off"), true);
assert.equal(configured.rootStyles.get("--dream-art-position"), "15% 80%");
assert.equal(configured.rootStyles.get("--dream-task-art"), 'url("blob:fixture-2")');
assert.equal(configured.rootStyles.get("--dream-accent"), "#d45a70");
assert.equal(configured.routeClasses.has("dream-home"), true);
assert.equal(configured.routeClasses.has("dream-task"), false);
assert.equal(configured.utilityClasses.has("dream-home-utility"), true);
assert.equal(configured.context.window.__CODEX_DREAM_SKIN_STATE__.cleanup(), true);
assert.equal(configured.utilityClasses.has("dream-home-utility"), false);

const angelina = createFixture({ shellPresent: true, homePresent: true });
const angelinaResult = vm.runInNewContext(buildPayload({
  variant: "angelina",
  appearance: "light",
  art: { focusX: .68, focusY: .42, safeArea: "left", taskMode: "ambient" },
}), angelina.context);
assert.equal(angelinaResult.version, "3.1.4-angelina");
assert.equal(angelina.rootClasses.has("dream-variant-angelina"), true);
const angelinaChrome = angelina.nodes.get("codex-dream-skin-chrome");
assert.equal(angelinaChrome.classList.contains("angelina-active"), true);
assert.match(angelinaChrome.innerHTML, /ANGELINA/);
assert.equal(angelinaChrome.style.left, "310px");
assert.equal(angelinaChrome.style.width, "910px");
assert.equal(angelina.context.window.__CODEX_DREAM_SKIN_STATE__.cleanup(), true);
assert.equal(angelina.rootClasses.has("dream-variant-angelina"), false);

const midnightAngelina = createFixture({ shellPresent: true, homePresent: true });
vm.runInNewContext(buildPayload({
  variant: "angelina",
  appearance: "dark",
  art: { focusX: .74, focusY: .42, safeArea: "left", taskMode: "ambient" },
}), midnightAngelina.context);
assert.equal(midnightAngelina.rootClasses.has("dream-variant-angelina"), true);
assert.equal(midnightAngelina.rootClasses.has("dream-theme-dark"), true);
assert.equal(midnightAngelina.rootClasses.has("dream-theme-light"), false);
assert.equal(midnightAngelina.rootStyles.get("--dream-art-position"), "74% 42%");
assert.equal(midnightAngelina.context.window.__CODEX_DREAM_SKIN_STATE__.cleanup(), true);

const analysisPixels = new Uint8ClampedArray(48 * 12 * 4);
for (let index = 0; index < 48 * 12; index += 1) {
  const offset = index * 4;
  const x = index % 48;
  const subject = x >= 34 && x <= 42;
  analysisPixels[offset] = subject ? 210 : 246;
  analysisPixels[offset + 1] = subject ? 84 : 239;
  analysisPixels[offset + 2] = subject ? 112 : 237;
  analysisPixels[offset + 3] = 255;
}
const analyzed = createFixture({
  shellPresent: true,
  analysisFixture: { naturalWidth: 1200, naturalHeight: 400, pixels: analysisPixels },
});
vm.runInNewContext(payload, analyzed.context);
await Promise.resolve();
assert.equal(analyzed.rootClasses.has("dream-theme-dark"), true);
assert.equal(analyzed.rootClasses.has("dream-theme-light"), false);
assert.equal(analyzed.rootClasses.has("dream-art-wide"), true);
assert.equal(analyzed.rootClasses.has("dream-task-banner"), true);
assert.equal(analyzed.rootClasses.has("dream-safe-left"), true);
assert.notEqual(analyzed.rootStyles.get("--dream-accent"), "rgb(216 104 119)");

const standardArt = createFixture({
  shellPresent: true,
  analysisFixture: { naturalWidth: 800, naturalHeight: 800, pixels: analysisPixels },
});
vm.runInNewContext(payload, standardArt.context);
await Promise.resolve();
assert.equal(standardArt.rootClasses.has("dream-art-standard"), true);
assert.equal(standardArt.rootClasses.has("dream-task-ambient"), true);
assert.equal(standardArt.rootClasses.has("dream-task-banner"), false);

const mediumWide = createFixture({
  shellPresent: true,
  analysisFixture: { naturalWidth: 2100, naturalHeight: 1000, pixels: analysisPixels },
});
vm.runInNewContext(payload, mediumWide.context);
await Promise.resolve();
assert.equal(mediumWide.rootClasses.has("dream-art-wide"), true);
assert.equal(mediumWide.rootClasses.has("dream-task-ambient"), true);
assert.equal(mediumWide.rootClasses.has("dream-task-banner"), false);

const nativeLight = createFixture({ shellPresent: true, shellAppearance: "light" });
vm.runInNewContext(payload, nativeLight.context);
assert.equal(nativeLight.rootClasses.has("dream-theme-light"), true);
assert.equal(nativeLight.rootClasses.has("dream-theme-dark"), false);

const nativeComputedDark = createFixture({
  shellPresent: true,
  shellAppearance: "",
  computedColorScheme: "dark",
  osAppearance: "light",
});
vm.runInNewContext(payload, nativeComputedDark.context);
assert.equal(nativeComputedDark.rootClasses.has("dream-theme-dark"), true);
assert.equal(nativeComputedDark.rootClasses.has("dream-theme-light"), false);
nativeComputedDark.context.window.__CODEX_DREAM_SKIN_STATE__.ensure();
assert.equal(nativeComputedDark.rootClasses.has("dream-theme-dark"), true);
const nativeObserver = nativeComputedDark.observers[0];
nativeObserver.takeRecords();
nativeComputedDark.context.window.__CODEX_DREAM_SKIN_STATE__.ensure();
assert.equal(nativeObserver.takeRecords().length, 0,
  "Sampling the native computed color-scheme must not queue a self-triggering root mutation pass.");

const metadataWide = createFixture({ shellPresent: true });
vm.runInNewContext(buildPayload({ artMetadata: { ratio: 16 / 9 } }), metadataWide.context);
assert.equal(metadataWide.rootClasses.has("dream-art-wide"), true);
assert.equal(metadataWide.rootClasses.has("dream-art-standard"), false);

console.log("PASS: renderer applies adaptive theme metadata, keeps skin without a sidebar, and preserves transparent auxiliary windows.");
