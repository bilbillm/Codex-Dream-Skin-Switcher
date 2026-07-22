# External Theme Packs

This launcher release intentionally ships without bundled themes. Import a local theme directory through the console, or copy a theme directory here and refresh the catalog.

The maintained Angelina light and dark packs live in [Codex Angelina Themes](https://github.com/bilbillm/Codex-Angelina-Themes). Theme directories contain data only: `theme.json` and local images. The launcher validates all image paths before it saves or injects a theme.

The launcher still owns trusted renderer adapters such as `variant: "angelina"`; theme packs cannot supply JavaScript or executable renderer code.
