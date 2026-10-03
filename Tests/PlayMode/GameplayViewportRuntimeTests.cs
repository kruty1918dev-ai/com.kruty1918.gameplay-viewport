using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kruty1918.GameplayViewport.Tests
{
    public sealed class GameplayViewportRuntimeTests
    {
        [UnityTest] public IEnumerator TextureResizeUpdatesOnceAndRefitsWithoutChangingObjects()
        {
            var root=new GameObject("adaptive",typeof(Camera),typeof(GameplayViewport),typeof(AdaptiveGameplayCamera));
            var camera=root.GetComponent<Camera>();camera.orthographic=true;camera.transform.rotation=Quaternion.Euler(50,30,0);
            var viewport=root.GetComponent<GameplayViewport>();var fit=root.GetComponent<AdaptiveGameplayCamera>();
            var texture=new RenderTexture(720,1600,16);camera.targetTexture=texture;
            var subject=new GameObject("game-owned");subject.transform.position=new Vector3(4,0,2);var original=subject.transform.position;
            var notifications=0;viewport.Changed+=_=>notifications++;
            try
            {
                yield return Frames(3);Assert.IsTrue(viewport.Frame.IsValid);Assert.AreEqual(1,notifications);
                var size=camera.orthographicSize;texture.Release();texture.width=2560;texture.height=1080;
                yield return Frames(3);Assert.AreEqual(2,notifications);Assert.IsTrue(viewport.Frame.IsLandscape);
                Assert.Less(camera.orthographicSize,size);Assert.AreEqual(original,subject.transform.position);
                yield return Frames(3);Assert.AreEqual(2,notifications);
                camera.orthographic=false;yield return Frames(3);Assert.Greater(camera.transform.position.magnitude,1);
                fit.SetWorldBounds(new Bounds(new Vector3(20,0,0),new Vector3(6,1,4)));yield return Frames(3);
                Assert.That(camera.WorldToViewportPoint(new Vector3(20,0,0)).x,Is.EqualTo(.5f).Within(.001));
            }
            finally{camera.targetTexture=null;Object.Destroy(texture);Object.Destroy(subject);Object.Destroy(root);}
            yield return null;
        }
        [UnityTest] public IEnumerator CameraPixelRectAndSafeAreaIntersectInPlayerWindowCoordinates()
        {
            var root=new GameObject("split",typeof(Camera),typeof(GameplayViewport));var camera=root.GetComponent<Camera>();camera.rect=new Rect(.5f,0,.5f,1);
            var viewport=root.GetComponent<GameplayViewport>();viewport.SafeAreaProvider=()=>new Rect(0,24,Screen.width-20,Screen.height-44);
            try
            {
                yield return Frames(3);Assert.IsTrue(viewport.Frame.IsValid);
                Assert.AreEqual(camera.pixelRect.xMin,viewport.Frame.SafePixels.xMin,.01f);
                Assert.AreEqual(Screen.width-20,viewport.Frame.SafePixels.xMax,.01f);
                Assert.AreEqual(24,viewport.Frame.SafePixels.yMin,.01f);
                viewport.SafeAreaProvider=()=>new Rect(0,0,0,0);viewport.Refresh();Assert.AreEqual(camera.pixelRect,viewport.Frame.GameplayPixels);
            }
            finally{Object.Destroy(root);}yield return null;
        }
        [UnityTest] public IEnumerator VisualAnchorTracksFramingOnPlaneWithoutChangingParent()
        {
            var root=new GameObject("camera",typeof(Camera),typeof(GameplayViewport),typeof(AdaptiveGameplayCamera));
            var camera=root.GetComponent<Camera>();camera.orthographic=true;camera.transform.rotation=Quaternion.Euler(70,0,0);
            var viewport=root.GetComponent<GameplayViewport>();viewport.RespectSafeArea=false;
            var actor=new GameObject("actor");actor.transform.position=new Vector3(9,0,3);var original=actor.transform.position;
            var visual=new GameObject("presentation");visual.transform.SetParent(actor.transform);
            var anchor=actor.AddComponent<ViewportWorldAnchor>();anchor.Viewport=viewport;anchor.Visual=visual.transform;anchor.Anchor=new Vector2(.2f,.4f);
            try
            {
                yield return Frames(3);Assert.AreEqual(original,actor.transform.position);Assert.AreEqual(0,visual.transform.position.y,.001f);
                Assert.That(camera.WorldToViewportPoint(visual.transform.position).x,Is.EqualTo(.2f).Within(.001));
                Assert.That(camera.WorldToViewportPoint(visual.transform.position).y,Is.EqualTo(.4f).Within(.001));
                anchor.Anchor=new Vector2(.8f,.7f);yield return Frames(3);
                Assert.That(camera.WorldToViewportPoint(visual.transform.position).x,Is.EqualTo(.8f).Within(.001));
                Assert.AreEqual(original,actor.transform.position);
            }
            finally{Object.Destroy(actor);Object.Destroy(root);}yield return null;
        }
        [UnityTest] public IEnumerator VisualSizingTracksProjectionClampsAndRestoresWithoutChangingActor()
        {
            var view = new GameObject("sizing camera", typeof(Camera), typeof(GameplayViewport));
            var camera = view.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5;
            var texture = new RenderTexture(720,1600,16); camera.targetTexture = texture; camera.aspect = 720f/1600;
            var actor = new GameObject("game actor", typeof(BoxCollider), typeof(ViewportVisualScale));
            actor.transform.position = new Vector3(0,0,10); var originalPosition = actor.transform.position;
            var visual = new GameObject("visual"); visual.transform.SetParent(actor.transform, false); visual.transform.localScale = Vector3.one * .2f;
            var size = actor.GetComponent<ViewportVisualScale>(); size.Viewport = view.GetComponent<GameplayViewport>(); size.Visual = visual.transform;
            size.ReferenceLocalScale = visual.transform.localScale; size.ReferenceWorldSize = .2f; size.SizeFraction = .05f;
            size.MinimumMultiplier = .5f; size.MaximumMultiplier = 10;
            try
            {
                yield return Frames(3); Assert.That(visual.transform.localScale.x, Is.EqualTo(.225f).Within(.001));
                camera.aspect = 2560f/1080; texture.Release(); texture.width = 2560; texture.height = 1080;
                yield return Frames(3); Assert.That(visual.transform.localScale.x, Is.EqualTo(.5f).Within(.001));
                camera.orthographic = false; camera.fieldOfView = 60; yield return Frames(3);
                var nearScale = visual.transform.localScale.x; actor.transform.position += Vector3.forward * 10; yield return Frames(3);
                Assert.That(visual.transform.localScale.x, Is.EqualTo(nearScale * 2).Within(.001));
                size.MaximumMultiplier = 1.5f; yield return Frames(3); Assert.That(visual.transform.localScale.x, Is.EqualTo(.3f).Within(.001));
                var logicalScale = actor.transform.localScale; size.Visual = actor.transform;
                Assert.IsFalse(size.ApplyNow()); Assert.AreEqual(logicalScale, actor.transform.localScale);
                size.Visual = visual.transform; size.enabled = false; Assert.AreEqual(Vector3.one * .2f, visual.transform.localScale);
                Assert.AreEqual(originalPosition + Vector3.forward * 10, actor.transform.position);
                Assert.AreEqual(Vector3.one, actor.transform.localScale); Assert.AreEqual(Vector3.one, actor.GetComponent<BoxCollider>().size);
            }
            finally { camera.targetTexture = null; Object.Destroy(texture); Object.Destroy(actor); Object.Destroy(view); }
            yield return null;
        }
        [UnityTest] public IEnumerator VisualVariantsRespondToRuntimeRootChangesAndRejectUnsafeConfiguration()
        {
            var view = new GameObject("variants camera", typeof(Camera), typeof(GameplayViewport));
            var texture = new RenderTexture(720,1600,16); view.GetComponent<Camera>().targetTexture = texture;
            var actor = new GameObject("variant owner", typeof(ViewportVisualVariants));
            var portrait = new GameObject("portrait"); var wide = new GameObject("wide"); var replacement = new GameObject("replacement");
            var variants = actor.GetComponent<ViewportVisualVariants>(); variants.Viewport = view.GetComponent<GameplayViewport>();
            variants.Variants = new[] { new ViewportVisualVariants.Variant { Root = portrait, MaximumAspect = 1 }, new ViewportVisualVariants.Variant { Root = wide, MinimumAspect = 1 } };
            try
            {
                yield return Frames(3); Assert.IsTrue(portrait.activeSelf); Assert.IsFalse(wide.activeSelf);
                // The selected index stays zero, but a runtime reference is replaced and was inactive.
                replacement.SetActive(false); variants.Variants[0].Root = replacement; yield return Frames(3); Assert.IsTrue(replacement.activeSelf);
                replacement.SetActive(false); yield return Frames(2); Assert.IsTrue(replacement.activeSelf);
                texture.Release(); texture.width = 2560; texture.height = 1080; yield return Frames(3);
                Assert.IsFalse(replacement.activeSelf); Assert.IsTrue(wide.activeSelf);
                variants.Variants[0].Root = actor; Assert.IsFalse(variants.ApplyNow()); Assert.IsTrue(actor.activeSelf); Assert.IsTrue(wide.activeSelf);
                variants.Variants[0].Root = wide; Assert.IsFalse(variants.ApplyNow()); Assert.IsTrue(wide.activeSelf);
                variants.Variants[0].Root = replacement; variants.Variants[1].MinimumAspect = float.NaN; Assert.IsFalse(variants.ApplyNow());
            }
            finally { Object.Destroy(texture); Object.Destroy(view); Object.Destroy(actor); Object.Destroy(portrait); Object.Destroy(wide); Object.Destroy(replacement); }
            yield return null;
        }
        static IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
    }
}
