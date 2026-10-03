using System;
using UnityEngine;

namespace Kruty1918.GameplayViewport
{
    /// <summary>One viewport per camera: safe area, aspect policy and optional UI reservation.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera)), DefaultExecutionOrder(1050)]
    public sealed class GameplayViewport : MonoBehaviour
    {
        public GameplayViewportProfile Profile;
        [Tooltip("Optional transparent UI rect describing the playable region. Intersected with the camera output and safe area.")]
        public RectTransform ContentViewport;
        public bool RespectSafeArea = true;
        public ViewportFrame Frame { get; private set; }
        public event Action<ViewportFrame> Changed;
        /// <summary>Optional host/device-simulator safe rectangle provider, in output pixels.</summary>
        public Func<Rect> SafeAreaProvider { get; set; }
        Camera _camera;
        readonly Vector3[] _corners = new Vector3[4];
        public Camera Camera => _camera != null ? _camera : (_camera = GetComponent<Camera>());
        void LateUpdate() => Refresh();
        /// <summary>Immediately capture geometry. Invalid/minimized outputs retain the last valid frame.</summary>
        public bool Refresh()
        {
            var camera = Camera; if (camera == null) return false;
            // Unity renders targetTexture into the whole texture, ignoring camera.rect.
            var texture = camera.targetTexture;
            var render = texture != null ? new Rect(0, 0, texture.width, texture.height) : camera.pixelRect;
            if (!ViewportGeometry.IsValid(render)) return false;
            var safe = render;
            if (RespectSafeArea && texture == null && camera.targetDisplay == 0)
            {
                var supplied = SafeAreaProvider != null ? SafeAreaProvider() : Screen.safeArea;
                if (ViewportGeometry.IsValid(supplied)) safe = ViewportGeometry.Intersect(render, supplied);
            }
            if (!ViewportGeometry.IsValid(safe)) return false;
            var playable = Profile != null ? Profile.Resolve(safe) : safe;
            if (ContentViewport != null)
            {
                if (texture != null) return false; // screen UI cannot describe a texture-space viewport
                var canvas = ContentViewport.GetComponentInParent<Canvas>()?.rootCanvas;
                if (canvas != null && canvas.targetDisplay != camera.targetDisplay) return false;
                var uiCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas != null ? canvas.worldCamera : camera;
                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay && uiCamera == null) return false;
                var min = new Vector2(float.MaxValue, float.MaxValue); var max = new Vector2(float.MinValue, float.MinValue);
                ContentViewport.GetWorldCorners(_corners);
                for (int i = 0; i < 4; i++)
                {
                    var point = RectTransformUtility.WorldToScreenPoint(uiCamera, _corners[i]);
                    min = Vector2.Min(min, point); max = Vector2.Max(max, point);
                }
                playable = ViewportGeometry.Intersect(playable, Rect.MinMaxRect(min.x, min.y, max.x, max.y));
            }
            if (!ViewportGeometry.IsValid(playable)) return false;
            var next = new ViewportFrame(render, safe, playable);
            if (Frame.Equals(next)) return false;
            Frame = next; Changed?.Invoke(Frame); return true;
        }
    }
}
