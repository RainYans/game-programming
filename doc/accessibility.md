# Accessibility, Security & Social Considerations

An honest account of how Monster Farm addresses (and where it falls short of) the legal, ethical,
social, accessibility, and security issues the brief asks for. The goal is a small, playable slice
that is considerate where it reasonably can be, with the gaps stated plainly rather than hidden.

## Accessibility

**What is supported**

- **Simple, conventional input.** Movement is **WASD**; interaction is a single context key (**E**);
  menus are mouse-driven. Combat adds **left-click** (attack), **right-click** (command the squad),
  **1/2** (items), **Left Shift** (dash), **Esc** (pause). No chords, no rapid timing inputs, no
  mouse-precision-dependent mechanics — the squad auto-fights, so the player is never forced into
  twitch execution.
- **Full key remapping.** Movement, interact, and dash keys can be **rebound** from the **Esc →
  Controls** screen; the bindings persist across sessions (`PlayerPrefs`) and can be reset to
  defaults at any time. Players who can't use the default WASD / E / Shift layout (one-handed play,
  alternative keyboard layouts) can set keys that work for them.
- **Volume controls.** An **Esc options menu** (farm) exposes three independent volume sliders —
  **Master**, **Music**, and **SFX** — persisted across sessions (`PlayerPrefs`), so players sensitive
  to sound can lower or mute each independently. SFX never rely on stereo position for information.
- **Readable UI.** Text uses a high-legibility pixel font (**Pixel Operator**) at large sizes on
  high-contrast parchment/dark panels. Important state is shown with **text + colour together**, not
  colour alone — e.g. a hungry unit shows the word **"(Hungry)"** as well as an orange tint; the
  deploy cards label state in text; HP is a bar **and** a number.
- **Clear goals / guidance.** A persistent **objective banner** states the current goal in both the
  farm and the raid; a first-launch onboarding and a dedicated combat **tutorial scene** teach the
  controls before the first real fight; a Help button / cheat-sheet lists every control.
- **No fail-punishment loops.** The farm loop has no timers or death; raids can be retried; the game
  never deletes progress on a loss beyond the deliberate permadeath of units the player chose to risk.
- **Forgiving difficulty knobs.** All combat balance lives in one `GameConfig` asset (and the new
  **Lab** lets players spend resources to strengthen strains), so a struggling player can grind safer
  upgrades rather than hitting a hard wall.

**Known gaps (not implemented in this slice)**

- **No colour-blind mode or text-size option.** Mitigated by the text-plus-colour rule above and a
  large base font, but there is no dedicated palette/scale toggle yet.
- **No subtitles** — there is no spoken dialogue, so this is not currently relevant; any future voiced
  content would need captions.
- **Fixed 1920×1080 tuning.** The UI is laid out for 1080p; other aspect ratios are untested.

## Security

The game is **offline and single-player**, which keeps the security surface very small:

- **No network, no accounts, no telemetry.** Nothing is sent or received; there is no attack surface
  from connectivity, and **no personal data is collected or stored** (no PII, no analytics).
- **Local save only.** State is written as plain JSON to Unity's `Application.persistentDataPath`
  via `SaveManager`. The file is trivially editable by the player — acceptable for a single-player
  game with no competitive or online stakes (the only "exploit" is editing your own save). Loading is
  **defensive**: an unreadable/corrupt save is caught and the game falls back to fresh-save defaults
  rather than crashing, and the save is **versioned + additive** so older saves load safely.
- No use of `eval`, dynamic code loading, or external processes at runtime.

## Legal / ethical

- **All third-party assets are licensed and credited** in [asset-credits.md](asset-credits.md): the
  Cute Fantasy art (Kenmi — kept local/git-ignored because its licence forbids redistributing the
  files), the fonts (Pixel Operator CC0, Alagard CC-BY), and the audio (Ninja Adventure SFX + three
  CC0 OpenGameArt music tracks). The **art is never re-hosted** in the public repo, per its licence.
- **Use of AI is disclosed** in [reference-and-contribution.md](reference-and-contribution.md): the C#
  was written with an AI assistant under my direction; the design, Unity construction, and decisions
  are mine.

## Social

- The tone is deliberately **cozy, hopeful, and non-graphic** — you raise creatures and "clear" feral
  monsters from villages; there is no gore, no human-on-human violence, no sexual content, and no
  real-world group is targeted. The fiction (reclaiming a corrupted valley) is gentle and broadly
  age-appropriate.
- No microtransactions, loot boxes, gambling mechanics, or dark patterns — the only economy is the
  in-game resource loop.
