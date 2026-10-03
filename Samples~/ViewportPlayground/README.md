# Viewport playground

Import this sample in Package Manager. In a **new empty scene**, create an empty GameObject and add `ViewportPlayground`. Enter Play Mode and resize the Game View from portrait to landscape/ultrawide. The camera fits a small procedural layout while all block world coordinates stay fixed. The sample creates a camera and directional light but no listener, input backend or game UI.

If your render pipeline requires a material, assign a compatible lit material to `BlockMaterial`. This sample is illustrative geometry, not production game assets. The runtime itself does not depend on this sample.

Try assigning an aspect profile to the generated camera's GameplayViewport. For UI reservations use a transparent screen-space RectTransform in your own Canvas and assign ContentViewport. For a procedural layout change call AdaptiveGameplayCamera.SetWorldBounds with the new bounds.

The presentation cue uses a gameplay-plane anchor and `ViewportVisualScale`. Its size follows the shortest playable span, clamped to 0.5–2 times the authored size. Resize the window and compare it with the fixed world blocks. The cue has no gameplay collider.
