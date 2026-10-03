# Quick start and concepts

The **render region** is one camera's pixel output. The **safe region** excludes unsafe screen edges for the primary display. The **gameplay region** is what remains after a profile and optional UI reservation. The world can still render behind a HUD; only its essential bounds are fitted into the gameplay region.

## Configure a camera

Add `GameplayViewport` to the camera, then `AdaptiveGameplayCamera`. The fitter requires the viewport automatically. Set camera rotation before fitting and author world-space `WorldBounds`. Assign `BoundsSpace` when the bounds are local to a moving/rotated object; its transform is included in the world AABB. `SetWorldBounds` clears `BoundsSpace` and invalidates fitting for procedural levels.

`Margin >= 1` is a framing margin. Include tent roofs, characters, tall gates or other required geometry in the bounds; the fitter cannot discover which scene objects are essential to your game.

Avoid including the entire background forest in a board game's bounds. Include only its playable clearing and a small aesthetic margin. Broader backgrounds can fill the output separately.

## Reserve space for a HUD

Make a transparent `RectTransform` fill the space where the level should appear. Its parent can already be a native safe-area container. Assign that rectangle to `ContentViewport`.

A bottom dock might leave the top/center of the phone free; a tablet dock might leave a wide region on its left. Your UI framework updates the rectangle; the package reads its resulting physical bounds. Safe area is intersected rather than added again, so there is no double padding.

RectTransforms rotated in screen space are conservatively represented by their axis-aligned screen rectangle. A nonrectangular UI opening needs a host-specific policy.

## Use a profile without a UI framework

Create **Assets → Create → Gameplay Viewport → Profile**. Insets use `(left, bottom, right, top)` fractions of the safe region.

Example: a portrait rule with `MinimumAspect=0`, `MaximumAspect=1.1`, insets `(0.02,0.20,0.02,0.12)` reserves a bottom dock and top controls. A wide rule starting at aspect `1.1`, insets `(0.02,0.04,0.28,0.08)`, reserves a right dock. The first matching rule wins; otherwise `DefaultInsets` applies.

Zero maximum aspect means unbounded. Minimum/maximum gameplay aspect can keep an ultrawide core readable without adding black bars or changing `Camera.rect`. Invalid/oversized inset values are sanitized and cannot collapse a valid region completely.

## Place optional visual objects

Add `ViewportWorldAnchor` to a presentation controller and assign a visual child, a viewport, normalized `Anchor` and a world plane. For a 3D ground plane, use `PlaneNormal=Vector3.up`. For a 2D XY scene, use `Vector3.forward`. An anchor `(0.5,0.5)` means the center of the gameplay region.

The anchor follows the final camera view/projection. It moves the assigned visual without changing its actor parent's position. Keep gameplay colliders outside that visual transform.

`ViewportVisualVariants` picks the first matching aspect range and activates that visual root. Keep its controller outside all variant roots, assign each root once, and call `Invalidate()` if replacing the array at runtime. These are visual compositions; gameplay content remains controlled by the game.
