((cssText, artDataUrl, taskArtDataUrl, backgroundArtDataUrl, foregroundArtDataUrl, rawConfig) => {
  const STATE_KEY = "__CODEX_DREAM_SKIN_STATE__";
  const STYLE_ID = "codex-dream-skin-style";
  const CHROME_ID = "codex-dream-skin-chrome";
  const PARALLAX_ID = "codex-dream-parallax";
  const PARALLAX_BACKGROUND_ID = "codex-dream-parallax-background";
  const PARALLAX_FOREGROUND_ID = "codex-dream-parallax-foreground";
  const LEGACY_PARALLAX_PROPERTIES = [
    "--dream-parallax-front-x",
    "--dream-parallax-front-y",
    "--dream-parallax-back-x",
    "--dream-parallax-back-y",
  ];
  const ROOT_CLASSES = [
    "codex-dream-skin",
    "dream-theme-light",
    "dream-theme-dark",
    "dream-art-wide",
    "dream-art-standard",
    "dream-focus-left",
    "dream-focus-center",
    "dream-focus-right",
    "dream-safe-left",
    "dream-safe-center",
    "dream-safe-right",
    "dream-safe-none",
    "dream-task-ambient",
    "dream-task-banner",
    "dream-task-off",
    "dream-parallax",
    "dream-variant-angelina",
  ];
  const ROOT_PROPERTIES = [
    "--dream-art",
    "--dream-task-art",
    "--dream-parallax-background-art",
    "--dream-parallax-foreground-art",
    "--dream-copy-parallax-x",
    "--dream-copy-parallax-y",
    "--dream-art-position",
    "--dream-art-position-x",
    "--dream-art-position-y",
    "--dream-parallax-front-x",
    "--dream-parallax-front-y",
    "--dream-parallax-back-x",
    "--dream-parallax-back-y",
    "--dream-focus-x",
    "--dream-focus-y",
    "--dream-accent",
    "--dream-accent-ink",
    "--dream-image-luma",
  ];
  const HOME_UTILITY_CLASS = "dream-home-utility";
  const SETTINGS_SHELL_CLASS = "dream-settings-shell";
  const SETTINGS_SEARCH_SELECTOR = [
    '[role="searchbox"][aria-label*="设置"]',
    '[role="searchbox"][aria-label*="settings" i]',
  ].join(", ");
  const installToken = {};
  let samplingNativeShell = false;
  let observer = null;
  let cachedNativeAppearance = null;
  let nativeAppearanceCheckedAt = 0;
  let lastProfileSignature = "";
  window.__CODEX_DREAM_SKIN_DISABLED__ = false;

  const clamp = (value, min = 0, max = 1) => Math.min(max, Math.max(min, Number(value)));
  const setStyleValue = (style, property, value) => {
    if (style?.[property] !== value) style[property] = value;
  };
  const luminance = (red, green, blue) => {
    const linear = [red, green, blue].map((value) => {
      const channel = value / 255;
      return channel <= .04045 ? channel / 12.92 : ((channel + .055) / 1.055) ** 2.4;
    });
    return .2126 * linear[0] + .7152 * linear[1] + .0722 * linear[2];
  };
  const defaultProfile = {
    appearance: "dark",
    accent: [108, 131, 142],
    focusX: .5,
    focusY: .5,
    aspect: 1.6,
    luma: .32,
    safeArea: "center",
  };

  const normalizeConfig = (value) => {
    const config = value && typeof value === "object" ? value : {};
    const art = config.art && typeof config.art === "object" ? config.art : {};
    const hasNumber = (candidate) =>
      (typeof candidate === "number" || (typeof candidate === "string" && candidate.trim() !== "")) &&
      Number.isFinite(Number(candidate));
    const requestedAccent = typeof config?.palette?.accent === "string"
      ? config.palette.accent.trim()
      : "";
    const safeAccent = /^(?:#[\da-f]{3,8}|(?:rgb|hsl|oklch|oklab)\([^;{}]{1,96}\))$/i.test(requestedAccent)
      ? requestedAccent
      : null;
    const appearance = ["auto", "light", "dark"].includes(config.appearance)
      ? config.appearance
      : "auto";
    const safeArea = ["auto", "left", "right", "center", "none"].includes(art.safeArea)
      ? art.safeArea
      : "auto";
    const taskMode = ["auto", "ambient", "banner", "off"].includes(art.taskMode)
      ? art.taskMode
      : "auto";
    const metadataRatio = Number(config?.artMetadata?.ratio);
    return {
      appearance,
      safeArea,
      taskMode,
      parallax: art.parallax === true,
      variant: config.variant === "angelina" ? "angelina" : null,
      focusX: hasNumber(art.focusX) ? clamp(art.focusX) : null,
      focusY: hasNumber(art.focusY) ? clamp(art.focusY) : null,
      accent: safeAccent,
      initialAspect: Number.isFinite(metadataRatio) && metadataRatio > 0 ? metadataRatio : null,
    };
  };

  const previous = window[STATE_KEY];
  if (previous?.observer) previous.observer.disconnect();
  if (previous?.timer) clearInterval(previous.timer);
  if (previous?.scheduler?.timeout) clearTimeout(previous.scheduler.timeout);
  previous?.parallax?.dispose?.();
  previous?.railJump?.dispose?.();
  previous?.clock?.dispose?.();
  previous?.rightPanelRelay?.dispose?.();
  for (const property of LEGACY_PARALLAX_PROPERTIES) {
    document.documentElement?.style.removeProperty(property);
  }
  if (previous?.artUrl) URL.revokeObjectURL(previous.artUrl);
  if (previous?.taskArtUrl && previous.taskArtUrl !== previous.artUrl) URL.revokeObjectURL(previous.taskArtUrl);
  if (previous?.backgroundArtUrl) URL.revokeObjectURL(previous.backgroundArtUrl);
  if (previous?.foregroundArtUrl) URL.revokeObjectURL(previous.foregroundArtUrl);
  const createArtUrl = (dataUrl) => {
    const comma = dataUrl.indexOf(",");
    const binary = atob(dataUrl.slice(comma + 1));
    const bytes = new Uint8Array(binary.length);
    for (let index = 0; index < binary.length; index += 1) bytes[index] = binary.charCodeAt(index);
    const mime = /^data:([^;,]+)/.exec(dataUrl)?.[1] || "image/png";
    return URL.createObjectURL(new Blob([bytes], { type: mime }));
  };
  const artUrl = createArtUrl(artDataUrl);
  const taskArtUrl = taskArtDataUrl ? createArtUrl(taskArtDataUrl) : artUrl;
  const backgroundArtUrl = backgroundArtDataUrl ? createArtUrl(backgroundArtDataUrl) : null;
  const foregroundArtUrl = foregroundArtDataUrl ? createArtUrl(foregroundArtDataUrl) : null;
  const config = normalizeConfig(rawConfig);
  const parallaxEnabled = config.parallax && Boolean(backgroundArtUrl && foregroundArtUrl);
  let lastParallaxState = null;
  const writeParallax = (x, y, force = false) => {
    const pixel = (value) => `${Math.round(value * 100) / 100}px`;
    const nextState = {
      copyX: pixel(x * 4),
      copyY: pixel(y * 2.5),
      foreground: `translate3d(${pixel(x * 10)}, ${pixel(y * 6)}, 0)`,
      background: `translate3d(${pixel(x * -5)}, ${pixel(y * -3)}, 0)`,
    };
    if (!force && lastParallaxState && Object.keys(nextState)
      .every((key) => nextState[key] === lastParallaxState[key])) return;
    const root = document.documentElement;
    root?.style.setProperty("--dream-copy-parallax-x", nextState.copyX);
    root?.style.setProperty("--dream-copy-parallax-y", nextState.copyY);
    const foreground = document.getElementById(PARALLAX_FOREGROUND_ID);
    const background = document.getElementById(PARALLAX_BACKGROUND_ID);
    if (foreground && background) {
      foreground.style.transform = nextState.foreground;
      background.style.transform = nextState.background;
    }
    lastParallaxState = nextState;
  };
  const createParallaxController = () => {
    writeParallax(0, 0);
    let frame = 0;
    let targetX = 0;
    let targetY = 0;
    const removeLayers = () => document.getElementById(PARALLAX_ID)?.remove();
    const ensureLayers = () => {
      if (!parallaxEnabled || !document.body) {
        removeLayers();
        return;
      }
      let container = document.getElementById(PARALLAX_ID);
      if (container?.parentElement === document.body) return;
      container?.remove();
      container = document.createElement("div");
      container.id = PARALLAX_ID;
      container.setAttribute("aria-hidden", "true");
      const background = document.createElement("div");
      background.id = PARALLAX_BACKGROUND_ID;
      const foreground = document.createElement("div");
      foreground.id = PARALLAX_FOREGROUND_ID;
      container.appendChild(background);
      container.appendChild(foreground);
      document.body.appendChild(container);
      writeParallax(targetX, targetY, true);
    };
    const reducedMotion = window.matchMedia?.("(prefers-reduced-motion: reduce)")?.matches;
    if (
      !parallaxEnabled ||
      reducedMotion ||
      typeof window.addEventListener !== "function" ||
      typeof window.requestAnimationFrame !== "function"
    ) {
      return { ensure: ensureLayers, dispose: removeLayers };
    }
    const render = () => {
      frame = 0;
      writeParallax(targetX, targetY);
    };
    const schedule = (x, y) => {
      if (x === targetX && y === targetY) return;
      targetX = x;
      targetY = y;
      if (!frame) frame = window.requestAnimationFrame(render);
    };
    const move = (event) => {
      const width = Math.max(1, Number(window.innerWidth) || 1);
      const height = Math.max(1, Number(window.innerHeight) || 1);
      const x = clamp((Number(event.clientX) / width) * 2 - 1, -1, 1);
      const y = clamp((Number(event.clientY) / height) * 2 - 1, -1, 1);
      schedule(x, y);
    };
    const reset = () => schedule(0, 0);
    window.addEventListener("pointermove", move, { passive: true });
    window.addEventListener("pointerleave", reset);
    window.addEventListener("blur", reset);
    return {
      ensure: ensureLayers,
      dispose() {
        window.removeEventListener("pointermove", move);
        window.removeEventListener("pointerleave", reset);
        window.removeEventListener("blur", reset);
        if (frame) window.cancelAnimationFrame?.(frame);
        frame = 0;
        removeLayers();
      },
    };
  };
  const parallax = createParallaxController();
  const createAngelinaClockController = () => {
    let timer = null;
    const dayNames = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];
    const pad = (value) => String(value).padStart(2, "0");
    const update = () => {
      const time = document.querySelector?.(".agl-clock-time");
      const date = document.querySelector?.(".agl-clock-date");
      if (!time || !date) return;
      const now = new Date();
      const timeText = `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`;
      const dateText = `${now.getFullYear()}.${pad(now.getMonth() + 1)}.${pad(now.getDate())} / ${dayNames[now.getDay()]}`;
      if (time.textContent !== timeText) time.textContent = timeText;
      if (date.textContent !== dateText) date.textContent = dateText;
    };
    const ensure = (visible) => {
      if (!visible) {
        if (timer) clearInterval(timer);
        timer = null;
        return;
      }
      update();
      if (!timer && typeof setInterval === "function") timer = setInterval(update, 1000);
    };
    return {
      ensure,
      dispose() {
        if (timer) clearInterval(timer);
        timer = null;
      },
    };
  };
  const clock = createAngelinaClockController();
  const createRailJumpController = () => {
    let timeout = null;
    const finish = () => {
      if (timeout) clearTimeout(timeout);
      timeout = null;
      document.querySelectorAll?.('[role="tooltip"][data-dream-rail-jump-active]')
        ?.forEach((tooltip) => tooltip.removeAttribute("data-dream-rail-jump-active"));
    };
    const begin = (event) => {
      if (!event.target?.closest?.("button[data-thread-user-message-navigation-item-id]")) return;
      document.querySelectorAll?.("[data-thread-user-message-navigation-tooltip-preview]")
        ?.forEach((preview) => preview.closest?.('[role="tooltip"]')
          ?.setAttribute("data-dream-rail-jump-active", ""));
      if (timeout) clearTimeout(timeout);
      timeout = setTimeout(finish, 650);
    };
    if (config.variant !== "angelina" || typeof document.addEventListener !== "function") {
      return { dispose: finish };
    }
    document.addEventListener("pointerdown", begin, true);
    document.addEventListener("click", begin, true);
    return {
      dispose() {
        document.removeEventListener?.("pointerdown", begin, true);
        document.removeEventListener?.("click", begin, true);
        finish();
      },
    };
  };
  const railJump = createRailJumpController();
  let profile = {
    ...defaultProfile,
    aspect: config.initialAspect ?? defaultProfile.aspect,
  };
  const existingStyle = document.getElementById(STYLE_ID);
  if (existingStyle) {
    existingStyle.textContent = cssText;
    existingStyle.dataset.dreamVersion = "3";
  }

  const analyzeArt = () => new Promise((resolve) => {
    if (typeof Image !== "function") {
      resolve(defaultProfile);
      return;
    }
    const image = new Image();
    image.onload = () => {
      try {
        const width = 48;
        const height = Math.max(12, Math.round(width * image.naturalHeight / image.naturalWidth));
        const canvas = document.createElement("canvas");
        canvas.width = width;
        canvas.height = height;
        const context = canvas.getContext?.("2d", { willReadFrequently: true });
        if (!context) throw new Error("Canvas is unavailable");
        context.drawImage(image, 0, 0, width, height);
        const pixels = context.getImageData(0, 0, width, height).data;
        let count = 0;
        let totalRed = 0;
        let totalGreen = 0;
        let totalBlue = 0;
        let totalBrightness = 0;
        const samples = [];
        const sampleMap = new Array(width * height);
        for (let offset = 0; offset < pixels.length; offset += 4) {
          if (pixels[offset + 3] < 96) continue;
          const red = pixels[offset];
          const green = pixels[offset + 1];
          const blue = pixels[offset + 2];
          const light = (.2126 * red + .7152 * green + .0722 * blue) / 255;
          const sample = { red, green, blue, light, index: offset / 4 };
          samples.push(sample);
          sampleMap[sample.index] = sample;
          totalRed += red;
          totalGreen += green;
          totalBlue += blue;
          totalBrightness += light;
          count += 1;
        }
        if (!count) throw new Error("Image contains no opaque pixels");
        const average = [totalRed / count, totalGreen / count, totalBlue / count];
        const averageBrightness = totalBrightness / count;
        const information = (start, end) => {
          let total = 0;
          let totalSquared = 0;
          let edges = 0;
          let edgeCount = 0;
          let sampleCount = 0;
          for (let y = 0; y < height; y += 1) {
            for (let x = start; x < end; x += 1) {
              const sample = sampleMap[y * width + x];
              if (!sample) continue;
              total += sample.light;
              totalSquared += sample.light * sample.light;
              sampleCount += 1;
              const previousSample = x > start ? sampleMap[y * width + x - 1] : null;
              const above = y > 0 ? sampleMap[(y - 1) * width + x] : null;
              if (previousSample) { edges += Math.abs(sample.light - previousSample.light); edgeCount += 1; }
              if (above) { edges += Math.abs(sample.light - above.light); edgeCount += 1; }
            }
          }
          const mean = sampleCount ? total / sampleCount : 0;
          const variance = sampleCount ? Math.max(0, totalSquared / sampleCount - mean * mean) : 1;
          return Math.sqrt(variance) * .58 + (edgeCount ? edges / edgeCount : 1) * .42;
        };
        const zoneWidth = Math.max(1, Math.floor(width * .38));
        const leftInformation = information(0, zoneWidth);
        const rightInformation = information(width - zoneWidth, width);
        let safeArea = "center";
        if (leftInformation < rightInformation * .86) safeArea = "left";
        else if (rightInformation < leftInformation * .86) safeArea = "right";
        let focusWeight = 0;
        let focusX = 0;
        let focusY = 0;
        let accentWeight = 0;
        let accent = [0, 0, 0];
        for (const sample of samples) {
          const x = sample.index % width;
          const y = Math.floor(sample.index / width);
          const difference = Math.sqrt(
            (sample.red - average[0]) ** 2 +
            (sample.green - average[1]) ** 2 +
            (sample.blue - average[2]) ** 2,
          ) / 441.7;
          const saliency = .03 + difference ** 1.35;
          focusX += (x / Math.max(1, width - 1)) * saliency;
          focusY += (y / Math.max(1, height - 1)) * saliency;
          focusWeight += saliency;
          const max = Math.max(sample.red, sample.green, sample.blue);
          const min = Math.min(sample.red, sample.green, sample.blue);
          const saturation = max ? (max - min) / max : 0;
          const usableLight = 1 - Math.min(1, Math.abs(sample.light - .46) / .54);
          const weight = saturation ** 2 * (.15 + usableLight);
          accent[0] += sample.red * weight;
          accent[1] += sample.green * weight;
          accent[2] += sample.blue * weight;
          accentWeight += weight;
        }
        const resolvedAccent = accentWeight > 1
          ? accent.map((channel) => Math.round(channel / accentWeight))
          : average.map((channel) => Math.round(channel));
        let resolvedFocusX = clamp(focusX / focusWeight);
        if (safeArea === "left") resolvedFocusX = Math.max(.64, resolvedFocusX);
        if (safeArea === "right") resolvedFocusX = Math.min(.36, resolvedFocusX);
        resolve({
          appearance: averageBrightness >= .58 ? "light" : "dark",
          accent: resolvedAccent,
          focusX: resolvedFocusX,
          focusY: clamp(focusY / focusWeight),
          aspect: image.naturalWidth / Math.max(1, image.naturalHeight),
          luma: clamp(averageBrightness),
          safeArea,
        });
      } catch {
        resolve(defaultProfile);
      }
    };
    image.onerror = () => resolve(defaultProfile);
    image.src = artUrl;
  });

  const detectShellAppearance = () => {
    const root = document.documentElement;
    const body = document.body;
    const classes = `${root?.className || ""} ${body?.className || ""}`
      .toLowerCase()
      .replace(/\bdream-theme-(?:dark|light)\b/g, "");
    if (/\b(dark|electron-dark|theme-dark|appearance-dark)\b/.test(classes)) return "dark";
    if (/\b(light|electron-light|theme-light|appearance-light)\b/.test(classes)) return "light";

    const dataTheme = (
      root?.getAttribute?.("data-theme") ||
      root?.getAttribute?.("data-appearance") ||
      root?.getAttribute?.("data-color-mode") ||
      body?.getAttribute?.("data-theme") ||
      body?.getAttribute?.("data-appearance") ||
      ""
    ).toLowerCase();
    if (dataTheme.includes("dark")) return "dark";
    if (dataTheme.includes("light")) return "light";

    try {
      const hadSkin = root?.classList?.contains?.("codex-dream-skin");
      const savedSkinClasses = hadSkin
        ? ROOT_CLASSES.filter((className) => root.classList.contains(className))
        : [];
      samplingNativeShell = true;
      if (hadSkin) root.classList.remove(...ROOT_CLASSES);
      try {
        const colorScheme = getComputedStyle(root).colorScheme || "";
        if (colorScheme.includes("dark") && !colorScheme.includes("light")) return "dark";
        if (colorScheme.includes("light") && !colorScheme.includes("dark")) return "light";
      } finally {
        if (hadSkin) root.classList.add(...savedSkinClasses);
        observer?.takeRecords?.();
        samplingNativeShell = false;
      }
    } catch {
      samplingNativeShell = false;
    }
    try {
      return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    } catch {}
    return "light";
  };

  const clearSkinDom = () => {
    const root = document.documentElement;
    root?.classList.remove(...ROOT_CLASSES);
    for (const property of ROOT_PROPERTIES) root?.style.removeProperty(property);
    document.querySelectorAll(".dream-home").forEach((node) => node.classList.remove("dream-home"));
    document.querySelectorAll(".dream-task").forEach((node) => node.classList.remove("dream-task"));
    document.querySelectorAll(".dream-home-shell").forEach((node) => node.classList.remove("dream-home-shell"));
    document.querySelectorAll(`.${SETTINGS_SHELL_CLASS}`).forEach((node) => node.classList.remove(SETTINGS_SHELL_CLASS));
    document.querySelectorAll(`.${HOME_UTILITY_CLASS}`).forEach((node) => node.classList.remove(HOME_UTILITY_CLASS));
    document.getElementById(STYLE_ID)?.remove();
    document.getElementById(CHROME_ID)?.remove();
    document.getElementById(PARALLAX_ID)?.remove();
  };

  const applyProfile = (root) => {
    const focusX = config.focusX ?? profile.focusX;
    const focusY = config.focusY ?? profile.focusY;
    const now = Date.now();
    const refreshNativeAppearance = config.appearance === "auto" && (
      !cachedNativeAppearance || now - nativeAppearanceCheckedAt >= 5000
    );
    if (refreshNativeAppearance) {
      cachedNativeAppearance = detectShellAppearance();
      nativeAppearanceCheckedAt = now;
    }
    const appearance = config.appearance === "auto" ? cachedNativeAppearance : config.appearance;
    const focus = focusX < .4 ? "left" : focusX > .6 ? "right" : "center";
    const safeArea = config.safeArea === "auto" ? (profile.safeArea ||
      (focus === "left" ? "right" : focus === "right" ? "left" : "center")) : config.safeArea;
    const taskMode = config.taskMode === "auto"
      ? profile.aspect >= 2.25 ? "banner" : "ambient"
      : config.taskMode;
    const accent = config.accent || `rgb(${profile.accent.join(" ")})`;
    const accentInk = luminance(...profile.accent) > .42 ? "rgb(26 24 28)" : "rgb(250 248 251)";
    const classStates = [
      ["dream-theme-light", appearance === "light"],
      ["dream-theme-dark", appearance === "dark"],
      ["dream-variant-angelina", config.variant === "angelina"],
      ["dream-parallax", parallaxEnabled],
      ["dream-art-wide", profile.aspect >= 1.75],
      ["dream-art-standard", profile.aspect < 1.75],
      ...["left", "center", "right"].map((value) => [`dream-focus-${value}`, focus === value]),
      ...["left", "center", "right", "none"].map((value) => [`dream-safe-${value}`, safeArea === value]),
      ...["ambient", "banner", "off"].map((value) => [`dream-task-${value}`, taskMode === value]),
    ];
    const propertyStates = [
      ["--dream-art", `url("${artUrl}")`],
      ["--dream-task-art", `url("${taskArtUrl}")`],
      ["--dream-art-position", `${Math.round(focusX * 100)}% ${Math.round(focusY * 100)}%`],
      ["--dream-art-position-x", `${Math.round(focusX * 100)}%`],
      ["--dream-art-position-y", `${Math.round(focusY * 100)}%`],
      ["--dream-focus-x", String(focusX)],
      ["--dream-focus-y", String(focusY)],
      ["--dream-accent", accent],
      ["--dream-accent-ink", accentInk],
      ["--dream-image-luma", profile.luma.toFixed(3)],
    ];
    if (parallaxEnabled) {
      propertyStates.push(
        ["--dream-parallax-background-art", `url("${backgroundArtUrl}")`],
        ["--dream-parallax-foreground-art", `url("${foregroundArtUrl}")`],
      );
    }
    const profileSignature = JSON.stringify([classStates, propertyStates]);
    const profileClassesMatch = classStates.every(([className, enabled]) =>
      root.classList.contains(className) === enabled);
    if (profileSignature === lastProfileSignature && profileClassesMatch) return;
    for (const [className, enabled] of classStates) root.classList.toggle(className, enabled);
    for (const [property, value] of propertyStates) root.style.setProperty(property, value);
    lastProfileSignature = profileSignature;
  };

  const ensure = () => {
    if (window.__CODEX_DREAM_SKIN_DISABLED__) return;
    const root = document.documentElement;
    if (!root || !document.body) return;

    // Main Codex shell is the content surface. The left rail is optional: Codex
    // removes or rebuilds aside.app-shell-left-panel while collapsing/expanding
    // it, and clearing the skin there flashes native colors over the active theme.
    // True auxiliary windows (pets, blank targets) still have no main surface, so
    // they continue to clear residual skin state.
    const shellMain = document.querySelector("main.main-surface") ||
      document.querySelector("main") ||
      document.querySelector('[role="main"]');
    if (!shellMain) {
      clearSkinDom();
      return;
    }

    if (!root.classList.contains("codex-dream-skin")) root.classList.add("codex-dream-skin");
    applyProfile(root);

    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement("style");
      style.id = STYLE_ID;
      (document.head || root).appendChild(style);
    }
    if (style.dataset.dreamVersion !== "4") {
      style.textContent = cssText;
      style.dataset.dreamVersion = "4";
    }
    parallax.ensure();

    const home = document.querySelector('[role="main"]:has([data-testid="home-icon"])') ||
      document.querySelector('[role="main"]:has([class~="group/home-suggestions"])') ||
      document.querySelector('[class~="group/home-suggestions"]')?.closest?.('[role="main"]') ||
      null;
    const mainCandidates = [...document.querySelectorAll('[role="main"]')];
    if (!mainCandidates.length) mainCandidates.push(shellMain);
    for (const candidate of mainCandidates) {
      candidate.classList.toggle("dream-home", candidate === home);
      candidate.classList.toggle("dream-task", candidate !== home);
    }
    const utilityBars = new Set(home ? home.querySelectorAll('[class*="_homeUtilityBar_"]') : []);
    for (const candidate of document.querySelectorAll(`.${HOME_UTILITY_CLASS}`)) {
      if (!utilityBars.has(candidate)) candidate.classList.remove(HOME_UTILITY_CLASS);
    }
    for (const candidate of utilityBars) candidate.classList.add(HOME_UTILITY_CLASS);
    shellMain.classList.toggle("dream-home-shell", Boolean(home));
    shellMain.classList.toggle(SETTINGS_SHELL_CLASS, Boolean(document.querySelector(SETTINGS_SEARCH_SELECTOR)));

    let chrome = document.getElementById(CHROME_ID);
    if (!chrome || chrome.parentElement !== document.body) {
      chrome?.remove();
      chrome = document.createElement("div");
      chrome.id = CHROME_ID;
      chrome.setAttribute("aria-hidden", "true");
      document.body.appendChild(chrome);
    }
    chrome.classList.toggle("dream-home-shell", Boolean(home));
    const hero = home?.querySelector?.(":scope > div:first-child > div:first-child > div:first-child") ?? null;
    const showAngelinaChrome = config.variant === "angelina" && Boolean(home && hero);
    chrome.classList.toggle("angelina-active", showAngelinaChrome);
    if (showAngelinaChrome) {
      const angelinaChrome = `
        <div class="agl-kicker"><b>ANGELINA</b><span>SR02 / GRAVITY FIELD</span></div>
        <div class="agl-quote">A slow messenger gets caught by the wind.</div>
        <div class="agl-clock"><b class="agl-clock-time">--:--:--</b><span class="agl-clock-date">----.--.-- / ---</span></div>`;
      const hasAngelinaChrome = chrome.querySelector?.(".agl-kicker") &&
        chrome.querySelector?.(".agl-quote") && chrome.querySelector?.(".agl-clock");
      if (!hasAngelinaChrome) chrome.innerHTML = angelinaChrome;
      const box = hero.getBoundingClientRect();
      setStyleValue(chrome.style, "left", `${Math.round(box.left)}px`);
      setStyleValue(chrome.style, "top", `${Math.round(box.top)}px`);
      setStyleValue(chrome.style, "width", `${Math.round(box.width)}px`);
      setStyleValue(chrome.style, "height", `${Math.round(box.height)}px`);
    } else if (chrome.innerHTML) {
      chrome.innerHTML = "";
    }
    clock.ensure(showAngelinaChrome);
  };

  const cleanup = () => {
    const state = window[STATE_KEY];
    if (state?.installToken !== installToken) return false;
    window.__CODEX_DREAM_SKIN_DISABLED__ = true;
    clearSkinDom();
    state?.observer?.disconnect();
    if (state?.timer) clearInterval(state.timer);
    if (state?.scheduler?.timeout) clearTimeout(state.scheduler.timeout);
    state?.parallax?.dispose?.();
    state?.railJump?.dispose?.();
    state?.clock?.dispose?.();
    if (state?.artUrl) URL.revokeObjectURL(state.artUrl);
    if (state?.taskArtUrl && state.taskArtUrl !== state.artUrl) URL.revokeObjectURL(state.taskArtUrl);
    if (state?.backgroundArtUrl) URL.revokeObjectURL(state.backgroundArtUrl);
    if (state?.foregroundArtUrl) URL.revokeObjectURL(state.foregroundArtUrl);
    delete window[STATE_KEY];
    return true;
  };

  const scheduler = { timeout: null };
  const scheduleEnsure = () => {
    if (scheduler.timeout) clearTimeout(scheduler.timeout);
    scheduler.timeout = setTimeout(() => {
      scheduler.timeout = null;
      ensure();
    }, 180);
  };
  observer = new MutationObserver(() => {
    if (samplingNativeShell) return;
    scheduleEnsure();
  });
  observer.observe(document.documentElement, {
    childList: true,
    subtree: true,
    attributes: true,
    attributeFilter: ["class", "data-theme", "data-appearance", "data-color-mode"],
  });
  const timer = setInterval(ensure, 5000);
  window[STATE_KEY] = {
    ensure, cleanup, observer, timer, scheduler, parallax, railJump, clock, artUrl, taskArtUrl,
    backgroundArtUrl, foregroundArtUrl, profile, config, installToken,
    version: "3.1.4-angelina",
  };
  ensure();
  analyzeArt().then((result) => {
    const state = window[STATE_KEY];
    if (state?.installToken !== installToken || window.__CODEX_DREAM_SKIN_DISABLED__) return;
    profile = result;
    state.profile = result;
    ensure();
  });
  return { installed: true, version: "3.1.4-angelina", adaptive: true };
})(
  __DREAM_CSS_JSON__,
  __DREAM_ART_JSON__,
  __DREAM_TASK_ART_JSON__,
  __DREAM_BACKGROUND_ART_JSON__,
  __DREAM_FOREGROUND_ART_JSON__,
  __DREAM_THEME_JSON__
)
