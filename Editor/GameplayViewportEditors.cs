using UnityEditor;
using UnityEngine;

namespace Kruty1918.GameplayViewport.Editor
{
    [CustomEditor(typeof(AdaptiveGameplayCamera))]
    public sealed class AdaptiveGameplayCameraEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var fit = (AdaptiveGameplayCamera)target;
            EditorGUILayout.HelpBox("This component owns camera position and framing. Use AutoApply=false or GameplayCameraMath for a camera owned by Cinemachine/another controller. Bounds are presentation metadata; game coordinates are unchanged.", MessageType.Info);
            if (GUILayout.Button("Use bounds of selected renderers"))
            {
                bool any = false; var bounds = new Bounds();
                foreach (var go in Selection.gameObjects)
                    foreach (var renderer in go.GetComponentsInChildren<Renderer>())
                    { if (!any) { bounds = renderer.bounds; any = true; } else bounds.Encapsulate(renderer.bounds); }
                if (any) { Undo.RecordObject(fit, "Set gameplay bounds"); fit.SetWorldBounds(bounds); EditorUtility.SetDirty(fit); }
                else EditorGUILayout.HelpBox("Select the gameplay renderers in the Hierarchy (lock this Inspector while selecting).",MessageType.Info);
            }
            if (Application.isPlaying && GUILayout.Button("Fit now")) fit.ApplyNow();
            if (fit.Viewport.Frame.IsValid)
                EditorGUILayout.LabelField("Playable pixels",fit.Viewport.Frame.GameplayPixels.ToString());
        }
    }
    [CustomEditor(typeof(ViewportVisualScale))]
    public sealed class ViewportVisualScaleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var scale = (ViewportVisualScale)target;
            if (scale.Visual == null || scale.Visual == scale.transform || scale.transform.IsChildOf(scale.Visual))
                EditorGUILayout.HelpBox("Assign a separate visual transform. The controller/actor and its ancestors cannot be scaled by this component.", MessageType.Error);
            else
            {
                if (scale.Visual.GetComponentsInChildren<Collider>(true).Length > 0)
                    EditorGUILayout.HelpBox("This visual contains colliders. Move gameplay colliders outside it before using presentation scaling.", MessageType.Warning);
                if (!Application.isPlaying && GUILayout.Button("Capture authored visual size"))
                {
                    var renderers = scale.Visual.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        var bounds = renderers[0].bounds;
                        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                        Undo.RecordObject(scale, "Capture visual reference size");
                        scale.ReferenceLocalScale = scale.Visual.localScale;
                        scale.ReferenceWorldSize = Mathf.Max(.0001f, bounds.size.x, bounds.size.y, bounds.size.z);
                        EditorUtility.SetDirty(scale);
                    }
                }
            }
            EditorGUILayout.HelpBox("SizeFraction is relative to the playable span at this object's depth. Limits preserve world proportions. Keep the parent scale stable; this is presentation sizing, not a physics or touch-target policy.", MessageType.Info);
        }
    }
    [CustomEditor(typeof(GameplayViewport))]
    public sealed class GameplayViewportEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var viewport = (GameplayViewport)target;
            if (viewport.Camera.targetTexture != null && viewport.ContentViewport != null)
                EditorGUILayout.HelpBox("Screen UI cannot reserve pixels in a RenderTexture. Clear ContentViewport and use a profile for texture-space margins.",MessageType.Error);
            if (viewport.Frame.IsValid)
            {
                EditorGUILayout.LabelField("Render pixels",viewport.Frame.RenderPixels.ToString());
                EditorGUILayout.LabelField("Safe pixels",viewport.Frame.SafePixels.ToString());
                EditorGUILayout.LabelField("Playable normalized",viewport.Frame.NormalizedGameplay.ToString());
            }
        }
        [MenuItem("GameObject/Gameplay Viewport/Adaptive Camera", false, 10)]
        static void Create(MenuCommand command)
        {
            var root = new GameObject("Adaptive Gameplay Camera",typeof(Camera),typeof(GameplayViewport),typeof(AdaptiveGameplayCamera));
            Undo.RegisterCreatedObjectUndo(root,"Create adaptive gameplay camera");
            GameObjectUtility.SetParentAndAlign(root,command.context as GameObject);
            root.GetComponent<Camera>().orthographic=true;
            root.transform.rotation=Quaternion.Euler(55,0,0);
            Selection.activeGameObject=root;
        }
    }
}
