# Verification and limits

Validated with Unity **6000.6.2f1**, Linux Editor. No physical phone, tablet or console benchmark is claimed.

Editor tests cover portrait phones (720×1600, 1080×1920), portrait/landscape tablets (1600×2560, 2048×1536), desktop (1920×1080), ultrawide (2560×1080), square and narrow output. Projected bounds are checked corner by corner for three rotations and three shapes in both camera modes. Offset gameplay regions, first-match profiles, invalid geometry, near-plane distance and aspect limits are covered.

PlayMode tests cover clamped visual sizing in both projection modes, disable restoration, runtime variant replacement/invalid configuration, resized RenderTextures with one geometry notification, orthographic/perspective fitting, procedural bounds updates, split pixel rectangles, custom safe area, and visual anchors that leave the gameplay parent unchanged. They passed in Quiet Camp's isolated QA project on 2026-10-03. Quiet Camp uses the math directly for its drifting/orbit cameras and native HUD reservations.

Enable package tests in a test project's `Packages/manifest.json`:

```json
"testables": ["com.kruty1918.gameplay-viewport"]
```

Run from a Unity project containing the package:

```sh
Unity -batchmode -nographics -projectPath /path/to/test-project -runTests -testPlatform EditMode -testFilter Kruty1918.GameplayViewport.Tests -testResults /tmp/viewport-editor.xml
Unity -projectPath /path/to/test-project -runTests -testPlatform PlayMode -testFilter Kruty1918.GameplayViewport.Tests -testResults /tmp/viewport-play.xml
```

`tools/check_package.py` and GitHub package checks validate packaging, metadata and documentation, not Unity compilation. Run Unity tests before release. Measure frame timing and allocations in the target game on actual hardware. This package does not promise an FPS target or solve content that is unreadable when zoomed far out.

Safe area belongs to the primary player window. Other displays, render textures and console output require the host's device/display policy. Screen UI reservations cannot be mixed with a RenderTexture output. Camera fitting does not solve arbitrary scene occlusion, interactive object density or game topology. Physics, save migration, supported orientations and camera ownership stay game responsibilities.

Version 0.2.0 was verified on 2026-10-03 with 20 package Editor cases and 5 package PlayMode cases, included in Quiet Camp’s 263/263 EditMode and 15/15 rendered integration/placement PlayMode checks. The imported sample was compiled in the test project. Game View checks include eight output shapes, uk/en/de, 130% text, retained menu scroll, album navigation and unchanged placement coordinates after resize. These are Editor results, not hardware certification.
