# Gameplay Viewport

[![Package checks](https://github.com/kruty1918dev-ai/com.kruty1918.gameplay-viewport/actions/workflows/package-check.yml/badge.svg)](https://github.com/kruty1918dev-ai/com.kruty1918.gameplay-viewport/actions/workflows/package-check.yml)
![UPM](https://img.shields.io/badge/Unity-UPM-blue)
![MIT](https://img.shields.io/badge/license-MIT-green)

Adaptive **gameplay framing and world presentation** for Unity 6. A reusable package for narrow phones, portrait tablets, desktop windows, ultrawide displays and standard console camera outputs.

Keep the important part of a level visible inside its available screen region. Fit an orthographic or perspective camera around world bounds, account for safe areas and UI reservations, and place optional visual objects using viewport anchors. The package has no dependency on Quiet Camp, HTML, Cinemachine, a render pipeline, an input backend or a platform SDK.

**Current version: 0.1.0.** Unity 6000.0+; validated with Unity 6000.6.2f1. See [verification](Documentation~/verification.md) for the tested scope and hardware limits.

## Install

Unity Package Manager → **+ → Install package from git URL**:

```text
https://github.com/kruty1918dev-ai/com.kruty1918.gameplay-viewport.git#v0.1.0
```

Or add to `Packages/manifest.json`:

```json
"com.kruty1918.gameplay-viewport": "https://github.com/kruty1918dev-ai/com.kruty1918.gameplay-viewport.git#v0.1.0"
```

Pin the full commit SHA in shipped games for reproducible builds. The runtime has no additional package dependencies. Normal Unity camera/UI engine modules must be enabled in the project.

## Start in two minutes

1. Add **GameplayViewport** and **AdaptiveGameplayCamera** to your camera. For a new scene, use **GameObject → Gameplay Viewport → Adaptive Camera**.
2. Set `WorldBounds` to cover your level. The Inspector can capture the bounds of selected renderers. Include roofs/raised objects if they must remain visible.
3. Keep your authored camera rotation. Choose orthographic for 2D/isometric play or perspective for 3D. The fitter adjusts position and orthographic size/distance while preserving this rotation.
4. Optionally assign a transparent `RectTransform` to `GameplayViewport.ContentViewport`. It describes the region left for gameplay after the HUD. Both safe area and camera output are intersected with it.
5. Resize or rotate the Game View. No game-specific resize loop is required.

For procedural levels:

```csharp
using Kruty1918.GameplayViewport;
using UnityEngine;

var viewport = camera.gameObject.AddComponent<GameplayViewport>();
viewport.ContentViewport = boardViewport; // optional screen UI reservation
var framing = camera.gameObject.AddComponent<AdaptiveGameplayCamera>();
framing.SetWorldBounds(new Bounds(levelCenter, levelSize));
```

For a scene that already owns its camera (orbit, drift, cutscenes, Cinemachine), use the [manual integration](Documentation~/integration.md). Do not give two systems ownership of the same camera pose.

## What adapts

| Feature | Behavior |
|---|---|
| Gameplay viewport | Camera pixel output, safe area, UI reservation, aspect policy |
| Orthographic camera | Contains projected bounds in the gameplay region, with optional margin |
| Perspective camera | Fits every bounds corner with a fixed vertical FOV and near-plane constraint |
| Portrait/landscape/ultrawide | Recalculates from geometry, without guessing device models |
| Split-screen | Each camera receives its own viewport-relative normalized region |
| RenderTexture | Uses texture dimensions; mobile notch insets are not applied |
| Visual world anchors | Places an opt-in visual on a plane using normalized gameplay coordinates |
| Visual variants | Chooses an opt-in visual composition by aspect range |
| UI systems | Optional RectTransform reservation works with uGUI/UnityHTML; custom UI can use profiles or a safe-area provider |

Gameplay coordinates, puzzle rules, object IDs, physics and save data remain owned by the game. The package does not silently rescale a level or move its colliders. `ViewportWorldAnchor` moves only the transform you explicitly assign.

## Learn more

- [Quick start and concepts](Documentation~/quick-start.md)
- [Camera ownership, procedural levels, UI and console integration](Documentation~/integration.md)
- [API and execution order](Documentation~/api.md)
- [Verification and supported scope](Documentation~/verification.md)
- [Changelog](CHANGELOG.md)
- [Contributing](CONTRIBUTING.md)

Import **Viewport Playground** from the Package Manager's Samples section, add `GameplayViewportPlayground` to an empty object and enter Play Mode. Resize the Game View to explore a procedural clearing and presentation anchor.

## Design choices

The core is small, typed C# with no reflection, telemetry, background service or platform permission. Geometry checks run without per-frame arrays/collections. Full camera fitting runs only when the viewport, bounds or camera parameters change; eight bounds corners are tested per fit. Use one viewport per gameplay camera, not one camera per decoration.

Profiles reserve normalized margins and optionally constrain gameplay aspect while scenery can continue rendering across the full output. A console game can configure TV action-safe margins explicitly. Secondary displays do not receive the primary display's notch inset.

The supplied fitter assumes a standard centered camera projection. Custom/off-axis projection, lens shift, XR stereo and Cinemachine lens ownership need a dedicated adapter; see integration guidance. Platform-compatible C# is not a claim of console SDK certification or measured mobile FPS.

## Tests

Add this package to your project's `testables`, then run the **GameplayViewport** suites in Unity Test Runner. Editor tests project every bounds corner through real Unity cameras over eight aspect ratios and three rotations in both projection modes. PlayMode tests cover viewport changes, unchanged-frame notifications, RenderTexture resizing, camera cropping, safe areas and visual anchors. The gameplay integration also runs Quiet Camp's UI, solver/save and placement regressions.

GitHub CI validates package metadata, source/meta pairing and documentation links without requiring a Unity license. Unity engine tests are run in an actual Unity installation; their scope is recorded separately in the verification document.

## License

[MIT](LICENSE). Copyright © 2026 Kruty1918.
