using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    public enum GameplaySpanAxis { Horizontal, Vertical, Shortest }

    /// <summary>World distance corresponding to a gameplay span at the object's camera depth.</summary>
    public static class GameplayPresentationMath
    {
        public static bool TryWorldSpan(Camera camera, ViewportFrame frame, Vector3 position,
            GameplaySpanAxis axis, out float span)
        {
            span = 0;
            if (camera == null || !frame.IsValid) return false;
            var p = camera.WorldToViewportPoint(position);
            if (!Finite(p) || p.z < camera.nearClipPlane) return false;
            var dx = Mathf.Abs(camera.WorldToViewportPoint(position + camera.transform.right).x - p.x);
            var dy = Mathf.Abs(camera.WorldToViewportPoint(position + camera.transform.up).y - p.y);
            if (!ViewportGeometry.Finite(dx) || !ViewportGeometry.Finite(dy) || dx < .000001f || dy < .000001f) return false;
            var region = frame.NormalizedGameplay;
            var width = region.width / dx; var height = region.height / dy;
            switch (axis)
            {
                case GameplaySpanAxis.Horizontal: span = width; break;
                case GameplaySpanAxis.Vertical: span = height; break;
                case GameplaySpanAxis.Shortest: span = Mathf.Min(width, height); break;
                default: return false;
            }
            return ViewportGeometry.Finite(span) && span > 0;
        }
        static bool Finite(Vector3 v) => ViewportGeometry.Finite(v.x) && ViewportGeometry.Finite(v.y) && ViewportGeometry.Finite(v.z);
    }

    /// <summary>Opt-in visual size relative to the playable region, without moving the actor.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(1350)]
    public sealed class ViewportVisualScale : MonoBehaviour
    {
        public GameplayViewport Viewport;
        [Tooltip("Required presentation transform. Keep gameplay colliders outside this transform.")]
        public Transform Visual;
        public GameplaySpanAxis Axis = GameplaySpanAxis.Shortest;
        [Min(.0001f)] public float SizeFraction = .025f;
        [Tooltip("World diameter/height at ReferenceLocalScale, with the authored parent scale.")]
        [Min(.0001f)] public float ReferenceWorldSize = 1;
        public Vector3 ReferenceLocalScale = Vector3.one;
        [Min(.0001f)] public float MinimumMultiplier = .5f;
        [Min(.0001f)] public float MaximumMultiplier = 2;
        public bool RestoreOnDisable = true;
        Transform _appliedTarget;
        Vector3 _originalScale;
        void LateUpdate() => ApplyNow();
        void OnDisable() { if (RestoreOnDisable) Restore(); }
        void Restore()
        {
            if (_appliedTarget != null) _appliedTarget.localScale = _originalScale;
            _appliedTarget = null;
        }
        public bool ApplyNow()
        {
            if (Visual == null || Visual == transform || transform.IsChildOf(Visual)
                || Viewport == null || !ViewportGeometry.Finite(SizeFraction) || SizeFraction <= 0
                || !ViewportGeometry.Finite(ReferenceWorldSize) || ReferenceWorldSize <= 0
                || !ViewportGeometry.Finite(MinimumMultiplier) || MinimumMultiplier <= 0
                || !ViewportGeometry.Finite(MaximumMultiplier) || MaximumMultiplier < MinimumMultiplier
                || !ViewportGeometry.Finite(ReferenceLocalScale.x) || !ViewportGeometry.Finite(ReferenceLocalScale.y) || !ViewportGeometry.Finite(ReferenceLocalScale.z)
                || !GameplayPresentationMath.TryWorldSpan(Viewport.Camera, Viewport.Frame, Visual.position, Axis, out var span)) return false;
            if (_appliedTarget != Visual)
            {
                Restore(); _appliedTarget = Visual; _originalScale = Visual.localScale;
            }
            var multiplier = Mathf.Clamp(span * SizeFraction / ReferenceWorldSize, MinimumMultiplier, MaximumMultiplier);
            var next = ReferenceLocalScale * multiplier;
            if (Visual.localScale != next) Visual.localScale = next;
            return true;
        }
    }
}
