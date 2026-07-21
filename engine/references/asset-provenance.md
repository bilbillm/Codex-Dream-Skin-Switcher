# Asset provenance

## `assets/angelina-hero.png`

- Purpose: home-screen hero for the Angelina Gravity Field Codex theme.
- Character reference: Angelina base artwork displayed by the community-maintained Arknights Terra Wiki, `https://arknights.wiki.gg/wiki/Angelina`.
- Generation path: Image-Studio Responses API shape, text model `gpt-5.6-sol`, image model `gpt-image-2`, image-to-image mode.
- Requested size: 2048 x 1152, explicit in the image tool payload.
- Provider response size: 1672 x 941; final asset was resampled with Lanczos to the required 2048 x 1152 and verified after writing.
- Editing: a second Responses edit corrected the lower-right staff-side hand to one thumb and four fingers while preserving the rest of the composition.
- Final format: RGB PNG, 2048 x 1152.

The theme is an unofficial personal fan customization. It is not affiliated with OpenAI, Hypergryph, or Arknights. Confirm applicable character and artwork rights before public redistribution or commercial use.

## `assets/angelina-thread-bg.jpg`

- Derived locally from the final hero image.
- Resized to 1280 x 720, desaturated slightly, reduced in contrast, and processed with a 28px Gaussian blur.
- Saved as an optimized progressive JPEG and displayed behind task/chat routes with additional low opacity, a 2px runtime blur, and a pearl-white scrim.

## `presets/preset-angelina-midnight-gravity/background.png`

- Purpose: independent dark home-screen wallpaper for the Angelina Midnight Gravity preset.
- Reference role: the finished bright Angelina hero was used only for character identity, original outfit, staff details, and right-weighted composition.
- Generation path: Image-Studio Responses API shape with explicit 2048 x 1152 size control, text model `gpt-5.5`, image model `gpt-image-2`, and the prompt in `references/angelina-midnight-prompt.txt`.
- Scene: a newly composed Rhodes Island operations terrace at midnight with moonlight, sparse city lights, cyan gravity arcs, floating messenger envelopes, and a dark low-detail UI-safe region on the left.
- Final format: RGB PNG, 2048 x 1152. The final image contains no UI, text, logo, signature, or watermark.

## `presets/preset-angelina-midnight-gravity/task-background.jpg`

- Derived locally from the final midnight wallpaper.
- Resized to 1280 x 720 and blurred/desaturated for task-page readability.
- Final format: RGB JPEG, 1280 x 720.
