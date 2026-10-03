# Integration

## One camera owner

`AdaptiveGameplayCamera` is an optional owner of camera position and framing. It keeps rotation authored by the host. Do not enable it alongside another controller that writes the same pose.

An existing orbit/drift controller can keep ownership and use the pure fitting API:

```csharp
viewport.Refresh();
var frame = viewport.Frame;
if (frame.IsValid) camera.aspect = frame.RenderAspect; // omit when the host owns a custom aspect
if (frame.IsValid && GameplayCameraMath.TryOrthographic(
    bounds, camera.transform.rotation, camera.aspect,
    frame.NormalizedGameplay, 20f, 1.05f, out var pose, camera.nearClipPlane))
{
    camera.orthographicSize = pose.OrthographicSize;
    camera.transform.position = pose.Position;
    camera.farClipPlane = Mathf.Max(camera.farClipPlane, pose.RequiredFarClip);
    // Apply your orbit/drift offset after the fitted base pose.
}
```

Use `TryPerspective` for a standard centered perspective camera. Its fit considers bounds depth and the near plane. Both APIs return `false` for invalid dimensions, NaN/infinite values or invalid projection inputs; the last valid pose can remain on screen. The game owns camera rotation, input, zoom limits and transition policy. An intentional zoom-in can crop content; that is separate from the containing fit.

For Cinemachine, feed the resulting geometry/pose to the appropriate camera extension/lens policy for the installed version, or let Cinemachine own framing and use only `GameplayViewport`. This package deliberately has no Cinemachine-version dependency.

The supplied camera math assumes centered projection. It does not model physical-camera gate fitting, lens shift, custom projection matrices or stereo/XR eye frusta. Do not use the standard fitter for those cameras without an adapter.

## Resize timing

Viewport capture runs at LateUpdate order 1050, after UnityHTML's layout watcher (1000). Automatic camera fitting runs at 1200; visual variants at 1250 and world anchors at 1300. A host can call `Refresh()`/`ApplyNow()` directly after its own layout or before capturing a frame.

The `Changed` event is emitted only for a valid, changed geometry snapshot. It means the **viewport** is ready; it precedes the optional automatic camera fitter. Use an anchor's later update or fit manually before reading final camera projection.

Invalid UI/zero-size output retains the last valid frame. If a temporarily unavailable view must also block input, that belongs to the game's input/transition policy.

## Camera outputs

- Normal window: use `Camera.pixelRect`; split-screen/cropped cameras have their own local normalized region.
- Secondary display: use that camera's output without the primary display's mobile safe area. The host still controls display activation and camera routing.
- RenderTexture: use the entire texture dimensions because Unity ignores `rect/pixelRect` while targeting a texture. A screen-space UI reservation cannot describe texture pixels; use a profile instead.
- Custom device simulator: supply `SafeAreaProvider` in player-window pixel coordinates. For a texture, the normal screen safe-area provider is intentionally ignored.

Unity API references: [Camera.pixelRect](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-pixelRect.html), [Camera.targetTexture](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-targetTexture.html), [Screen.safeArea](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Screen-safeArea.html).

## Phones, tablets, desktop and consoles

Adaptation is based on camera/window geometry, not platform names or diagonal screen size. Rotation, a foldable window or a desktop resize produces a new frame using the same math. Profile margins can reserve TV action-safe edges for a console game without installing a console SDK in the package. The game controls supported orientations, minimum desktop window size and display mode.

A very large level cannot remain simultaneously readable and fully visible on every screen. Use level framing bounds, game-owned scrolling/zoom or a profile with appropriate core aspect limits. Changing rules/level topology to fit a screen is outside this package.

## Quiet Camp integration

Quiet Camp's `CameraFitter` is a small domain adapter: it turns a level's width/height into bounds and passes the native HUD viewport to `GameplayViewport`. Its camera fitting now uses `GameplayCameraMath` in the gameplay scene, main menu and album. The menu's drift and album's orbit retain camera ownership; their visual offsets are applied after a containing fit. The meadow and atmosphere fit to the resulting camera. Puzzle cells, tent colliders, pathfinding, generated content and saves retain their coordinates.

UnityHTML remains responsible for UI layout; this package is independent and can be used in games with any presentation framework.

## Keep a small world cue readable

Add `ViewportVisualScale` to the actor/controller and assign only its visual child. Capture the authored size in the Inspector. Choose a fraction of the horizontal, vertical or shortest gameplay span, then bound the scale multiplier. For example, Quiet Camp's door cue has a 0.14-unit reference, 0.019 shortest-span fraction and multiplier limits 1–1.7. The tent root, door position, collider and saved cell do not change. New tents receive the same policy after placement/undo/redo, and album tents use their shared orbit camera.

Use this for small landmarks, directional cues and opted-in presentation props. Keep world architecture at game-owned scale. This component does not create touch hit areas. Parent scaling/shear and occlusion need host-specific policy. Position an optional cue with ViewportWorldAnchor first; sizing runs afterward.

The world-span calculation samples [Unity camera projection](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-projectionMatrix.html) at the visual depth; the gameplay frame contributes only its reserved fraction. It works in standard orthographic and perspective views without using physical device DPI.
