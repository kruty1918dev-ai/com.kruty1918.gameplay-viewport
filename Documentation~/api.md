# API reference

Namespace: `Kruty1918.GameplayViewport`. Runtime assembly: `Kruty1918.GameplayViewport`.

| Type | Main API | Responsibility |
| --- | --- | --- |
| GameplayViewport | Frame, Changed, Refresh(), Camera | Capture valid output geometry; emit only when geometry changes |
| GameplayViewportProfile | AspectRules, Resolve(...) | First matching aspect range, insets and min/max gameplay aspect |
| ViewportFrame | RenderPixels, SafePixels, GameplayPixels, NormalizedGameplay, RenderAspect, GameplayAspect | Immutable geometry in output-pixel and camera-normalized coordinates |
| GameplayCameraMath | TryOrthographic(...), TryPerspective(...) | Pure containing fit; returns false for invalid inputs |
| GameplayCameraPose | Position, OrthographicSize, RequiredFarClip | Recommended camera pose and far-plane extent |
| AdaptiveGameplayCamera | SetWorldBounds(...), ApplyNow(), Invalidate() | Optional automatic camera pose and output-aspect owner |
| ViewportWorldAnchor | ApplyNow(), Visual, Anchor, PlaneOrigin, PlaneNormal, WorldOffset | Opt-in visual position from a gameplay anchor intersected with a world plane |
| ViewportVisualVariants | Variants, Invalidate() | Activate the first matching visual composition |

`GameplayViewport.ContentViewport` optionally accepts a screen UI `RectTransform`. `RespectSafeArea` controls physical safe bounds. `SafeAreaProvider` supplies simulator/window-pixel bounds. Invalid or temporarily empty geometry retains the last valid frame; `Refresh()` returns whether a new frame was published.

`AdaptiveGameplayCamera.WorldBounds` are world space unless `BoundsSpace` is assigned. `SetWorldBounds` clears that local-space reference. Margin is a size multiplier. Camera rotation and vertical perspective FOV stay authored. The far plane only grows when ExpandFarClip is enabled. Disable AutoApply when calling ApplyNow from your own camera loop.

`MatchOutputAspect` defaults to true. This synchronizes projection to the output, including a RenderTexture whose dimensions change in place. Set it false to preserve a manually supplied `camera.aspect`. The fitter then uses the actual authored projection aspect.

Profile inset ordering is left, bottom, right, top; values are fractions of the safe region. Zero maximum aspect means unbounded. Aspect ranges and visual variants use first-match ordering; the maximum is exclusive. Variants must have distinct roots and cannot contain the controller that switches them.

Execution orders: viewport 1050, fitter 1200, variants 1250, anchors 1300. Input rules and UI layout remain with the host game.
