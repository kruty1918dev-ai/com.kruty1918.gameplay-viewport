# Contributing

Open an issue with Unity version, camera mode, output size, pixel rect, safe area and a minimal reproduction. Include before/after screenshots for framing changes. Remove private assets and credentials from examples.

Keep the runtime independent of render pipeline, game rules, input backend and UI framework. Visual adaptation must be explicit; do not move gameplay colliders, rewrite levels or alter saves automatically. Preserve existing camera integrations and allocation-free per-frame geometry paths.

Run `python3 tools/check_package.py`. Install the package by local path in a Unity 6 test project, add it to `manifest.json` testables, and run `Kruty1918.GameplayViewport.Tests` in EditMode and PlayMode. Test portrait, landscape, square, ultrawide, a resized RenderTexture and split-screen. See Documentation~/verification.md for commands and coverage.

Pull requests should describe the triggering geometry, resulting behavior and validation. New API or behavior needs documentation; keep hardware claims bounded by actual measurements.
