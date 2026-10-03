using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.GameplayViewport.Tests
{
    public sealed class GameplayViewportMathTests
    {
        [TestCase(720,1600)] [TestCase(1080,1920)] [TestCase(1600,2560)] [TestCase(2560,1600)]
        [TestCase(2048,1536)] [TestCase(2560,1080)] [TestCase(1024,768)] [TestCase(1600,720)]
        public void OrthographicAndPerspectiveKeepEveryBoundsCornerInReservedRegion(int width,int height)
        {
            var go=new GameObject("fit-test",typeof(Camera));var camera=go.GetComponent<Camera>();
            try
            {
                camera.aspect=(float)width/height;camera.nearClipPlane=.3f;camera.farClipPlane=1000;
                var reserved=new Rect(.05f,.12f,width>height?.65f:.90f,.72f);
                foreach(var rotation in new[]{Quaternion.Euler(58,225,0),Quaternion.identity,Quaternion.Euler(20,65,12)})
                    foreach(var size in new[]{new Vector3(4,.8f,8),new Vector3(8,5,4),new Vector3(15,3,20)})
                    {
                        camera.transform.rotation=rotation;var bounds=new Bounds(new Vector3(13,4,-8),size);
                        Assert.IsTrue(GameplayCameraMath.TryOrthographic(bounds,rotation,camera.aspect,reserved,20,1.03f,out var ortho));
                        camera.orthographic=true;camera.orthographicSize=ortho.OrthographicSize;camera.transform.position=ortho.Position;
                        AssertContained(camera,bounds,reserved);
                        camera.orthographic=false;camera.fieldOfView=48;
                        Assert.IsTrue(GameplayCameraMath.TryPerspective(bounds,rotation,camera.aspect,reserved,48,.3f,1.03f,out var perspective));
                        camera.transform.position=perspective.Position;AssertContained(camera,bounds,reserved);
                    }
            }
            finally{Object.DestroyImmediate(go);}
        }
        static void AssertContained(Camera camera,Bounds bounds,Rect region)
        {
            var e=bounds.extents;
            for(int i=0;i<8;i++)
            {
                var corner=bounds.center+new Vector3((i&1)==0?-e.x:e.x,(i&2)==0?-e.y:e.y,(i&4)==0?-e.z:e.z);
                var p=camera.WorldToViewportPoint(corner);
                Assert.Greater(p.z,camera.nearClipPlane);Assert.Less(p.z,camera.farClipPlane);
                Assert.That(p.x,Is.InRange(region.xMin-.0001f,region.xMax+.0001f));
                Assert.That(p.y,Is.InRange(region.yMin-.0001f,region.yMax+.0001f));
            }
        }
        [Test] public void SplitScreenSafeIntersectionNormalizesRelativeToCamera()
        {
            var render=new Rect(960,0,960,1080);var safe=new Rect(0,24,1900,1036);
            var frame=new ViewportFrame(render,ViewportGeometry.Intersect(render,safe),ViewportGeometry.Intersect(render,safe));
            Assert.AreEqual(new Rect(960,24,940,1036),frame.GameplayPixels);
            Assert.AreEqual(940f/960,frame.NormalizedGameplay.width,.0001f);
            Assert.AreEqual(0,frame.NormalizedGameplay.x);Assert.IsTrue(frame.IsValid);
        }
        [Test] public void AspectProfilesPickFirstMatchAndStayPlayableForInvalidInsets()
        {
            var profile=ScriptableObject.CreateInstance<GameplayViewportProfile>();
            try
            {
                profile.Rules=new[]{new GameplayViewportProfile.AspectRule{MinimumAspect=1.1f,Insets=new Vector4(0,0,.3f,0)}};
                Assert.AreEqual(new Rect(0,0,1400,1000),profile.Resolve(new Rect(0,0,2000,1000)));
                Assert.AreEqual(new Rect(0,0,1000,2000),profile.Resolve(new Rect(0,0,1000,2000)));
                profile.DefaultInsets=new Vector4(2,float.NaN,2,-1);
                Assert.IsTrue(ViewportGeometry.IsValid(profile.Resolve(new Rect(0,0,1000,2000))));
            }
            finally{Object.DestroyImmediate(profile);}
        }
        [Test] public void AspectConstraintPreservesCenterAndFitsInsideInput()
        {
            var input=new Rect(10,20,3000,1000);var result=ViewportGeometry.ConstrainAspect(input,0,1.8f);
            Assert.AreEqual(input.center,result.center);Assert.AreEqual(1.8f,result.width/result.height,.0001f);
            Assert.AreEqual(result,ViewportGeometry.Intersect(result,input));
            Assert.AreEqual(input,ViewportGeometry.ConstrainAspect(input,1,0),"A zero maximum must remain unbounded");
        }
        [Test] public void InvalidGeometryAndBoundsAreRejectedWithoutPose()
        {
            Assert.IsFalse(ViewportGeometry.IsValid(new Rect(0,0,float.NaN,300)));
            Assert.IsFalse(GameplayCameraMath.TryOrthographic(new Bounds(Vector3.zero,Vector3.one),Quaternion.identity,0,new Rect(0,0,1,1),20,1,out _));
            Assert.IsFalse(GameplayCameraMath.TryPerspective(new Bounds(Vector3.zero,Vector3.one),Quaternion.identity,1,new Rect(-.1f,0,1,1),60,.1f,1,out _));
            Assert.IsFalse(GameplayCameraMath.TryPerspective(new Bounds(Vector3.zero,Vector3.one),Quaternion.identity,1,new Rect(0,0,1,1),float.NaN,.1f,1,out _));
            Assert.AreEqual(default(Rect),ViewportGeometry.Intersect(new Rect(0,0,1,1),new Rect(2,2,1,1)));
        }
        [Test] public void DeepOrthographicBoundsRespectNearClip()
        {
            Assert.IsTrue(GameplayCameraMath.TryOrthographic(new Bounds(Vector3.zero,new Vector3(2,2,100)),Quaternion.identity,1,new Rect(0,0,1,1),5,1,out var pose,3));
            Assert.Greater(pose.Distance,53);Assert.Greater(pose.RequiredFarClip,103);
        }
    }
}
