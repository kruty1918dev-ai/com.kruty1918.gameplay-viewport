# Changelog

## 0.2.0 — 2026-10-03

- Add opt-in `ViewportVisualScale` and projection-aware world-span math for visual cues across portrait, landscape and ultrawide views.
- Clamp size changes around an authored reference; restore the original visual scale when disabled; leave actor coordinates unchanged.
- Re-evaluate runtime visual variant/root changes, reject nested/duplicate/controller roots and invalid ranges before applying any changes.
- Add visual reference capture and collider guidance in the Inspector; extend the importable playground with a positioned and sized cue.
- Quiet Camp uses clamped sizing for tent door cues and matches manual camera projection to the captured output.


## 0.1.0 — 2026-10-03

Initial Unity 6 UPM release.

- Camera-relative safe area, UI reservations, aspect profiles, split output and RenderTexture geometry.
- Orthographic and perspective bounds fitting, including offset gameplay regions and near-plane protection.
- Automatic output-aspect matching; opt out with `MatchOutputAspect = false` when another system sets the projection aspect.
- Opt-in world visual anchors and aspect composition variants.
- Inspector setup, procedural playground, integration and API documentation.
- Editor geometry tests and PlayMode resize/anchor tests; integrated in Quiet Camp.

Physical devices and console hardware have not been certified. See the verification document.
