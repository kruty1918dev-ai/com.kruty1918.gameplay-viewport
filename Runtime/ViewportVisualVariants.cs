using System;
using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    /// <summary>Opt-in visual composition variants, chosen by aspect rather than device class.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(1250)]
    public sealed class ViewportVisualVariants : MonoBehaviour
    {
        public GameplayViewport Viewport;
        public Variant[] Variants = Array.Empty<Variant>();
        void LateUpdate() => ApplyNow();
        /// <summary>Validate and apply the current configuration, including runtime root replacements.</summary>
        public bool ApplyNow()
        {
            if (Viewport == null || !Viewport.Frame.IsValid || Variants == null) return false;
            // A variant must not disable its controller or refer to one root twice.
            for (int i = 0; i < Variants.Length; i++)
            {
                var root = Variants[i]?.Root; if (root == null) continue;
                if (transform == root.transform || transform.IsChildOf(root.transform)) return false;
                for (int j = 0; j < i; j++)
                {
                    var other = Variants[j]?.Root;
                    if (other != null && (root.transform.IsChildOf(other.transform) || other.transform.IsChildOf(root.transform))) return false;
                }
                var variant = Variants[i];
                if (!ViewportGeometry.Finite(variant.MinimumAspect) || variant.MinimumAspect < 0
                    || !ViewportGeometry.Finite(variant.MaximumAspect) || variant.MaximumAspect < 0
                    || (variant.MaximumAspect > 0 && variant.MaximumAspect <= variant.MinimumAspect)) return false;
            }
            var aspect = Viewport.Frame.GameplayAspect; var selected = -1;
            for (int i = 0; i < Variants.Length; i++)
                if (Variants[i] != null && aspect >= Variants[i].MinimumAspect && (Variants[i].MaximumAspect <= 0 || aspect < Variants[i].MaximumAspect)) { selected = i; break; }
            for (int i = 0; i < Variants.Length; i++)
                if (Variants[i]?.Root != null && Variants[i].Root.activeSelf != (i == selected)) Variants[i].Root.SetActive(i == selected);
            return true;
        }
        public void Invalidate() { } // Retained for source compatibility; configuration is checked every update.
        [Serializable] public sealed class Variant
        {
            public GameObject Root;
            [Min(0)] public float MinimumAspect;
            [Tooltip("Zero means unbounded.")] [Min(0)] public float MaximumAspect;
        }
    }
}
