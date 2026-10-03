using UnityEngine;
using Kruty1918.GameplayViewport;

namespace Kruty1918.GameplayViewport.Samples
{
    public sealed class ViewportPlayground : MonoBehaviour
    {
        public Material BlockMaterial;
        void Awake()
        {
            var view = new GameObject("Adaptive sample camera", typeof(Camera), typeof(GameplayViewport), typeof(AdaptiveGameplayCamera));
            view.transform.SetParent(transform, false);
            var camera = view.GetComponent<Camera>();
            camera.orthographic = true; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.18f,.25f,.21f);
            camera.transform.rotation = Quaternion.Euler(55,30,0);
            view.GetComponent<AdaptiveGameplayCamera>().SetWorldBounds(new Bounds(transform.position,new Vector3(12,2,8)));
            for (int x=0;x<5;x++) for (int z=0;z<3;z++)
            {
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = "World block " + x + ":" + z;
                block.transform.SetParent(transform,false);
                block.transform.localPosition = new Vector3(x*2-4,.3f,z*2-2);
                block.transform.localScale = new Vector3(1.4f,.6f,1.4f);
                Destroy(block.GetComponent<BoxCollider>());
                // Explicit engine references keep primitive components in players.
                var mesh = block.GetComponent<MeshFilter>();
                var renderer = block.GetComponent<MeshRenderer>();
                if (mesh != null && BlockMaterial != null) renderer.sharedMaterial = BlockMaterial;
            }
            var sun = new GameObject("Sample sunlight",typeof(Light));sun.transform.SetParent(transform,false);
            sun.GetComponent<Light>().type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(45,35,0);
        }
    }
}
