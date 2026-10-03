using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.GameplayViewport.Tests
{
    public sealed class GameplayPresentationMathTests
    {
        [TestCase(true, 720, 1600)] [TestCase(false, 720, 1600)]
        [TestCase(true, 2048, 1536)] [TestCase(false, 2048, 1536)]
        [TestCase(true, 2560, 1080)] [TestCase(false, 2560, 1080)]
        public void WorldSpanProjectsBackToReservedDimension(bool orthographic, int width, int height)
        {
            var root = new GameObject("span camera", typeof(Camera)); var camera = root.GetComponent<Camera>();
            try
            {
                camera.orthographic = orthographic; camera.orthographicSize = 8; camera.aspect = (float)width / height;
                camera.transform.SetPositionAndRotation(new Vector3(5, 7, -10), Quaternion.Euler(30, 25, 0));
                var output = new Rect(0, 0, width, height);
                var frame = new ViewportFrame(output, output, new Rect(width * .1f, height * .2f, width * .65f, height * .7f));
                foreach (var depth in new[] { 3f, 20f })
                {
                    var position = camera.transform.position + camera.transform.forward * depth + camera.transform.right * .4f;
                    Assert.IsTrue(GameplayPresentationMath.TryWorldSpan(camera, frame, position, GameplaySpanAxis.Horizontal, out var horizontal));
                    Assert.IsTrue(GameplayPresentationMath.TryWorldSpan(camera, frame, position, GameplaySpanAxis.Vertical, out var vertical));
                    Assert.IsTrue(GameplayPresentationMath.TryWorldSpan(camera, frame, position, GameplaySpanAxis.Shortest, out var shortest));
                    var origin = camera.WorldToViewportPoint(position);
                    Assert.That(camera.WorldToViewportPoint(position + camera.transform.right * horizontal).x - origin.x, Is.EqualTo(.65f).Within(.0001));
                    Assert.That(camera.WorldToViewportPoint(position + camera.transform.up * vertical).y - origin.y, Is.EqualTo(.7f).Within(.0001));
                    Assert.That(shortest, Is.EqualTo(Mathf.Min(horizontal, vertical)).Within(.0001));
                }
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void InvalidOrBehindCameraSpanIsRejected()
        {
            var root = new GameObject("span camera", typeof(Camera)); var camera = root.GetComponent<Camera>();
            try
            {
                var rect = new Rect(0,0,720,1600); var frame = new ViewportFrame(rect,rect,rect);
                Assert.IsFalse(GameplayPresentationMath.TryWorldSpan(camera, frame, Vector3.back, GameplaySpanAxis.Shortest, out _));
                Assert.IsFalse(GameplayPresentationMath.TryWorldSpan(camera, frame, new Vector3(float.NaN,0,10), GameplaySpanAxis.Shortest, out _));
                Assert.IsFalse(GameplayPresentationMath.TryWorldSpan(camera, default, Vector3.forward * 10, GameplaySpanAxis.Shortest, out _));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
