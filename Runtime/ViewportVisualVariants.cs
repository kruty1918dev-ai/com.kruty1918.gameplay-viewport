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
        int _selected = -2;
        void OnEnable() => Invalidate();
        void OnValidate() => Invalidate();
        void LateUpdate()
        {
            if (Viewport == null || !Viewport.Frame.IsValid || Variants == null) return;
            // A variant must not disable its controller or refer to one root twice.
            for (int i = 0; i < Variants.Length; i++)
            {
                var root = Variants[i]?.Root; if (root == null) continue;
                if (transform == root.transform || transform.IsChildOf(root.transform)) return;
                for (int j = 0; j < i; j++) if (Variants[j]?.Root == root) return;
            }
            var aspect = Viewport.Frame.GameplayAspect; var selected = -1;
            for (int i = 0; i < Variants.Length; i++)
                if (Variants[i] != null && aspect >= Variants[i].MinimumAspect && (Variants[i].MaximumAspect <= 0 || aspect < Variants[i].MaximumAspect)) { selected = i; break; }
            if (selected == _selected) return;
            _selected = selected;
            for (int i = 0; i < Variants.Length; i++)
                if (Variants[i]?.Root != null) Variants[i].Root.SetActive(i == selected);
        }
        public void Invalidate() => _selected = -2;
        [Serializable] public sealed class Variant
        {
            public GameObject Root;
            [Min(0)] public float MinimumAspect;
            [Tooltip("Zero means unbounded.")] [Min(0)] public float MaximumAspect;
        }
    }
}
