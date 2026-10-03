using System;
using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    /// <summary>Immutable pixel geometry of one camera output and its playable region.</summary>
    public readonly struct ViewportFrame : IEquatable<ViewportFrame>
    {
        public readonly Rect RenderPixels, SafePixels, GameplayPixels, NormalizedGameplay;
        public bool IsValid => ViewportGeometry.IsValid(RenderPixels) && ViewportGeometry.IsValid(GameplayPixels);
        public float RenderAspect => RenderPixels.width / Mathf.Max(1, RenderPixels.height);
        public float GameplayAspect => GameplayPixels.width / Mathf.Max(1, GameplayPixels.height);
        public bool IsLandscape => RenderPixels.width > RenderPixels.height;
        public ViewportFrame(Rect render, Rect safe, Rect gameplay)
        {
            RenderPixels = render; SafePixels = safe; GameplayPixels = gameplay;
            NormalizedGameplay = ViewportGeometry.Normalize(gameplay, render);
        }
        public bool Equals(ViewportFrame other) => RenderPixels == other.RenderPixels && SafePixels == other.SafePixels && GameplayPixels == other.GameplayPixels;
        public override bool Equals(object obj) => obj is ViewportFrame other && Equals(other);
        public override int GetHashCode() => RenderPixels.GetHashCode() ^ SafePixels.GetHashCode() ^ GameplayPixels.GetHashCode();
    }
    public static class ViewportGeometry
    {
        public static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool IsValid(Rect rect) => Finite(rect.x) && Finite(rect.y) && Finite(rect.width) && Finite(rect.height) && rect.width > .01f && rect.height > .01f;
        public static Rect Intersect(Rect a, Rect b)
        {
            if (!IsValid(a) || !IsValid(b)) return default;
            var left = Mathf.Max(a.xMin, b.xMin); var bottom = Mathf.Max(a.yMin, b.yMin);
            var right = Mathf.Min(a.xMax, b.xMax); var top = Mathf.Min(a.yMax, b.yMax);
            return right > left && top > bottom ? Rect.MinMaxRect(left, bottom, right, top) : default;
        }
        public static Rect Normalize(Rect inner, Rect outer)
        {
            if (!IsValid(inner) || !IsValid(outer)) return default;
            return new Rect((inner.x - outer.x) / outer.width, (inner.y - outer.y) / outer.height,
                inner.width / outer.width, inner.height / outer.height);
        }
        /// <param name="fractions">Left, bottom, right, top, relative to the input rectangle.</param>
        public static Rect Inset(Rect rect, Vector4 fractions)
        {
            if (!IsValid(rect)) return default;
            var left = Clean(fractions.x); var bottom = Clean(fractions.y);
            var right = Clean(fractions.z); var top = Clean(fractions.w);
            // Invalid user configuration must not collapse an otherwise playable view.
            var horizontal = left + right; var vertical = bottom + top;
            if (horizontal > .9f) { left *= .9f / horizontal; right *= .9f / horizontal; }
            if (vertical > .9f) { bottom *= .9f / vertical; top *= .9f / vertical; }
            return Rect.MinMaxRect(rect.xMin + rect.width * left, rect.yMin + rect.height * bottom,
                rect.xMax - rect.width * right, rect.yMax - rect.height * top);
        }
        static float Clean(float value) => Finite(value) ? Mathf.Clamp(value, 0, .9f) : 0;
        public static Rect ConstrainAspect(Rect rect, float minAspect, float maxAspect)
        {
            if (!IsValid(rect)) return default;
            var aspect = rect.width / rect.height;
            var min = Finite(minAspect) ? Mathf.Max(0, minAspect) : 0;
            var max = Finite(maxAspect) && maxAspect > 0 ? Mathf.Max(min, maxAspect) : 0;
            if (min > 0 && aspect < min) return new Rect(rect.x, rect.center.y - rect.width / min * .5f, rect.width, rect.width / min);
            if (max > 0 && aspect > max) return new Rect(rect.center.x - rect.height * max * .5f, rect.y, rect.height * max, rect.height);
            return rect;
        }
    }
}
