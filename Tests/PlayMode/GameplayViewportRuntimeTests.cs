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
        static IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
    }
}
