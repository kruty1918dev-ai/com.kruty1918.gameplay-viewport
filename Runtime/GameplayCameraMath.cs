using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    public readonly struct GameplayCameraPose
    {
        public readonly Vector3 Position;
        public readonly float OrthographicSize, Distance, RequiredFarClip;
        public GameplayCameraPose(Vector3 position, float size, float distance, float far)
        { Position = position; OrthographicSize = size; Distance = distance; RequiredFarClip = far; }
    }
    /// <summary>Allocation-free fitting; retains camera rotation, gameplay coordinates and colliders.</summary>
    public static class GameplayCameraMath
    {
        public static bool TryOrthographic(Bounds bounds, Quaternion rotation, float renderAspect, Rect playable,
            float distance, float margin, out GameplayCameraPose pose, float nearClip = .1f)
        {
            pose = default;
            if (!Valid(bounds, rotation, renderAspect, playable, margin) || !ViewportGeometry.Finite(distance) || distance <= 0 || !ViewportGeometry.Finite(nearClip) || nearClip <= 0) return false;
            float maxX = 0, maxY = 0, maxZ = 0;
            var inverse = Quaternion.Inverse(rotation); var extent = bounds.extents * margin;
            for (int i = 0; i < 8; i++)
            {
                var corner = inverse * Corner(extent, i);
                maxX = Mathf.Max(maxX, Mathf.Abs(corner.x)); maxY = Mathf.Max(maxY, Mathf.Abs(corner.y)); maxZ = Mathf.Max(maxZ, Mathf.Abs(corner.z));
            }
            var size = Mathf.Max(.001f, Mathf.Max(maxX / (renderAspect * playable.width), maxY / playable.height));
            // Keep every bounds corner in front of the camera, even for deep worlds.
            distance = Mathf.Max(distance, maxZ + nearClip + .01f);
            var local = new Vector3((1 - 2 * playable.center.x) * size * renderAspect,
                (1 - 2 * playable.center.y) * size, -distance);
            pose = new GameplayCameraPose(bounds.center + rotation * local, size, distance, distance + maxZ + 1);
            return Finite(pose.Position) && ViewportGeometry.Finite(size);
        }
        public static bool TryPerspective(Bounds bounds, Quaternion rotation, float renderAspect, Rect playable,
            float verticalFieldOfView, float nearClip, float margin, out GameplayCameraPose pose)
        {
            pose = default;
            if (!Valid(bounds, rotation, renderAspect, playable, margin) || !ViewportGeometry.Finite(verticalFieldOfView)
                || verticalFieldOfView < 1 || verticalFieldOfView > 175 || !ViewportGeometry.Finite(nearClip) || nearClip <= 0) return false;
            var tanY = Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * .5f); var tanX = tanY * renderAspect;
            var inverse = Quaternion.Inverse(rotation); var extent = bounds.extents * margin;
            var left = (2 * playable.xMin - 1) * tanX; var right = (2 * playable.xMax - 1) * tanX;
            var bottom = (2 * playable.yMin - 1) * tanY; var top = (2 * playable.yMax - 1) * tanY;
            float distance = .001f, maxZ = 0;
            for (int i = 0; i < 8; i++)
            {
                var c = inverse * Corner(extent, i); maxZ = Mathf.Max(maxZ, c.z);
                distance = Mathf.Max(distance, nearClip + .01f - c.z);
                distance = Mathf.Max(distance, (c.x - right * c.z) / (playable.width * tanX));
                distance = Mathf.Max(distance, (-c.x + left * c.z) / (playable.width * tanX));
                distance = Mathf.Max(distance, (c.y - top * c.z) / (playable.height * tanY));
                distance = Mathf.Max(distance, (-c.y + bottom * c.z) / (playable.height * tanY));
            }
            var local = new Vector3((1 - 2 * playable.center.x) * tanX * distance,
                (1 - 2 * playable.center.y) * tanY * distance, -distance);
            pose = new GameplayCameraPose(bounds.center + rotation * local, 0, distance, distance + maxZ + 1);
            return Finite(pose.Position) && ViewportGeometry.Finite(distance);
        }
        static Vector3 Corner(Vector3 half, int i) => new Vector3((i & 1) == 0 ? -half.x : half.x,
            (i & 2) == 0 ? -half.y : half.y, (i & 4) == 0 ? -half.z : half.z);
        static bool Finite(Vector3 v) => ViewportGeometry.Finite(v.x) && ViewportGeometry.Finite(v.y) && ViewportGeometry.Finite(v.z);
        static bool Valid(Bounds b, Quaternion q, float aspect, Rect rect, float margin)
        {
            var length = q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w;
            return Finite(b.center) && Finite(b.size) && b.size.x >= 0 && b.size.y >= 0 && b.size.z >= 0
                && ViewportGeometry.Finite(length) && Mathf.Abs(length - 1) < .01f
                && ViewportGeometry.Finite(aspect) && aspect > .001f && ViewportGeometry.IsValid(rect)
                && rect.xMin >= 0 && rect.yMin >= 0 && rect.xMax <= 1.0001f && rect.yMax <= 1.0001f
                && ViewportGeometry.Finite(margin) && margin >= 1;
        }
    }
}
