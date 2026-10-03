using System;
using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    [CreateAssetMenu(menuName = "Gameplay Viewport/Profile", fileName = "ViewportProfile")]
    public sealed class GameplayViewportProfile : ScriptableObject
    {
        [Tooltip("Left, bottom, right, top fractions of the safe region. Also useful for TV action-safe margins.")]
        public Vector4 DefaultInsets;
        [Tooltip("First matching rule wins. Device names and platform guesses are not used.")]
        public AspectRule[] Rules = Array.Empty<AspectRule>();
        [Min(0)] public float MinimumGameplayAspect;
        [Min(0)] public float MaximumGameplayAspect;
        public Rect Resolve(Rect safe)
        {
            var insets = DefaultInsets;
            var aspect = safe.width / Mathf.Max(.01f, safe.height);
            if (Rules != null)
                foreach (var rule in Rules)
                    if (rule != null && rule.Matches(aspect)) { insets = rule.Insets; break; }
            return ViewportGeometry.ConstrainAspect(ViewportGeometry.Inset(safe, insets), MinimumGameplayAspect, MaximumGameplayAspect);
        }
        [Serializable] public sealed class AspectRule
        {
            public string Name = "Wide";
            [Min(0)] public float MinimumAspect = 1.1f;
            [Tooltip("Zero means no upper bound.")] [Min(0)] public float MaximumAspect;
            public Vector4 Insets;
            public bool Matches(float aspect) => aspect >= MinimumAspect && (MaximumAspect <= 0 || aspect < MaximumAspect);
        }
    }
}
