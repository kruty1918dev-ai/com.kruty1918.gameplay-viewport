using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    /// <summary>Opt-in presentation object placed on a world plane using the playable viewport.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(1300)]
    public sealed class ViewportWorldAnchor : MonoBehaviour
    {
        public GameplayViewport Viewport;
        [Tooltip("Zero = bottom-left, one = top-right of the playable region.")]
        public Vector2 Anchor = new Vector2(.5f, .5f);
        public Vector3 PlaneOrigin;
        public Vector3 PlaneNormal = Vector3.up;
        public Vector3 WorldOffset;
        [Tooltip("Optional visual child. Game-owned colliders and puzzle coordinates should remain outside this transform.")]
        public Transform Visual;
        Matrix4x4 _lastView, _lastProjection;
        ViewportFrame _lastFrame;
        Vector2 _lastAnchor;
        Vector3 _lastOrigin, _lastNormal, _lastOffset;
        Transform _lastTarget;
        void LateUpdate()
        {
            if (Viewport == null || !Viewport.Frame.IsValid) return;
            var camera = Viewport.Camera;
            var target = Visual != null ? Visual : transform;
            if (_lastFrame.Equals(Viewport.Frame) && _lastView == camera.worldToCameraMatrix && _lastProjection == camera.projectionMatrix
                && Anchor == _lastAnchor && PlaneOrigin == _lastOrigin && PlaneNormal == _lastNormal && WorldOffset == _lastOffset && target == _lastTarget) return;
            ApplyNow();
        }
        static bool Finite(Vector3 v) => ViewportGeometry.Finite(v.x) && ViewportGeometry.Finite(v.y) && ViewportGeometry.Finite(v.z);
        public bool ApplyNow()
        {
            if (Viewport == null || !Viewport.Frame.IsValid || PlaneNormal.sqrMagnitude < .0001f
                || !Finite(PlaneOrigin) || !Finite(PlaneNormal) || !Finite(WorldOffset) || !ViewportGeometry.Finite(Anchor.x) || !ViewportGeometry.Finite(Anchor.y)) return false;
            var camera = Viewport.Camera; var region = Viewport.Frame.NormalizedGameplay;
            var screen = new Vector3(region.xMin + Mathf.Clamp01(Anchor.x) * region.width, region.yMin + Mathf.Clamp01(Anchor.y) * region.height, 0);
            var ray = camera.ViewportPointToRay(screen); var plane = new Plane(PlaneNormal.normalized, PlaneOrigin);
            if (!plane.Raycast(ray, out var distance) || !ViewportGeometry.Finite(distance)) return false;
            var target = Visual != null ? Visual : transform; target.position = ray.GetPoint(distance) + WorldOffset;
            _lastFrame = Viewport.Frame; _lastView = camera.worldToCameraMatrix; _lastProjection = camera.projectionMatrix;
            _lastAnchor = Anchor; _lastOrigin = PlaneOrigin; _lastNormal = PlaneNormal; _lastOffset = WorldOffset; _lastTarget = target;
            return true;
        }
    }
}
