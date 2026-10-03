using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    /// <summary>Optional camera owner. For other camera systems use GameplayCameraMath directly.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayViewport)), DefaultExecutionOrder(1200)]
    public sealed class AdaptiveGameplayCamera : MonoBehaviour
    {
        [Tooltip("Static world-space bounds. Use SetWorldBounds for procedurally generated levels.")]
        public Bounds WorldBounds = new Bounds(Vector3.zero, new Vector3(8, 2, 8));
        [Tooltip("Optional transform: WorldBounds is then local to this object, including rotation and scale.")]
        public Transform BoundsSpace;
        [Min(1)] public float Margin = 1.05f;
        [Min(.01f)] public float OrthographicDistance = 20;
        public bool AutoApply = true;
        public bool ExpandFarClip = true;
        [Tooltip("Keep projection aligned with output pixels, including resized RenderTextures. Disable when another system owns camera.aspect.")]
        public bool MatchOutputAspect = true;
        GameplayViewport _viewport;
        ViewportFrame _lastFrame;
        Bounds _lastBounds;
        Quaternion _lastRotation;
        float _lastFov, _lastMargin, _lastDistance, _lastNear, _lastAspect;
        bool _lastOrthographic, _lastMatchOutputAspect;
        bool _dirty = true;
        public GameplayViewport Viewport => _viewport != null ? _viewport : (_viewport = GetComponent<GameplayViewport>());
        public void SetWorldBounds(Bounds bounds) { WorldBounds = bounds; BoundsSpace = null; _dirty = true; }
        public void Invalidate() => _dirty = true;
        void LateUpdate()
        {
            if (!AutoApply) return;
            var camera = Viewport.Camera; var bounds = ResolveBounds();
            if (_dirty || !Viewport.Frame.Equals(_lastFrame) || !bounds.Equals(_lastBounds) || camera.transform.rotation != _lastRotation
                || camera.orthographic != _lastOrthographic || MatchOutputAspect != _lastMatchOutputAspect || camera.aspect != _lastAspect || camera.fieldOfView != _lastFov || Margin != _lastMargin || OrthographicDistance != _lastDistance || camera.nearClipPlane != _lastNear)
                ApplyNow();
        }
        public bool ApplyNow()
        {
            Viewport.Refresh(); var frame = Viewport.Frame; if (!frame.IsValid) return false;
            var camera = Viewport.Camera; var bounds = ResolveBounds(); GameplayCameraPose pose;
            if (MatchOutputAspect) camera.aspect = frame.RenderAspect;
            var fitted = camera.orthographic
                ? GameplayCameraMath.TryOrthographic(bounds, camera.transform.rotation, camera.aspect, frame.NormalizedGameplay, OrthographicDistance, Margin, out pose, camera.nearClipPlane)
                : GameplayCameraMath.TryPerspective(bounds, camera.transform.rotation, camera.aspect, frame.NormalizedGameplay, camera.fieldOfView, camera.nearClipPlane, Margin, out pose);
            if (!fitted) return false;
            camera.transform.position = pose.Position;
            if (camera.orthographic) camera.orthographicSize = pose.OrthographicSize;
            if (ExpandFarClip) camera.farClipPlane = Mathf.Max(camera.farClipPlane, pose.RequiredFarClip);
            _lastFrame = frame; _lastBounds = bounds; _lastRotation = camera.transform.rotation;
            _lastFov = camera.fieldOfView; _lastMargin = Margin; _lastDistance = OrthographicDistance; _lastNear = camera.nearClipPlane; _lastAspect = camera.aspect; _lastOrthographic = camera.orthographic; _lastMatchOutputAspect=MatchOutputAspect; _dirty = false;
            return true;
        }
        Bounds ResolveBounds()
        {
            if (BoundsSpace == null) return WorldBounds;
            var c = BoundsSpace.TransformPoint(WorldBounds.center); var e = WorldBounds.extents;
            var x = BoundsSpace.TransformVector(e.x,0,0); var y = BoundsSpace.TransformVector(0,e.y,0); var z = BoundsSpace.TransformVector(0,0,e.z);
            var extent = Abs(x) + Abs(y) + Abs(z);
            return new Bounds(c, extent * 2);
        }
        static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
    }
}
