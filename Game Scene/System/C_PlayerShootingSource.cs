using PainterTool;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_PlayerShootingSource : MonoBehaviour, IPEGI
    {
        public Brush brush = new Brush();

        bool TryHit(out RaycastHit hit) 
        {
            var cam = Camera.main;
            if (!cam) 
            {
                hit = new RaycastHit();
                return false;
            }
            
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                return Physics.Raycast(new Ray(transform.position, hit.point - transform.position), out hit);
            }

            return false;
        }

        public void Paint(Vector3 target)
        {
            RaycastHit hit;

            if (Physics.Raycast(new Ray(transform.position, target - transform.position), out hit))
            {
                var receivers = hit.transform.GetComponentsInParent<C_PaintingReceiver>();

                Singleton.Try<Singleton_SmokeEffectController>(s => s.TrySpawnIfVisible(hit.point, out _));

                Singleton.Try<Singleton_ImpactLightsController>(s => s.TrySpawnIfVisible(hit.point, out _));

                if (receivers.Length == 0)
                    return;

                int subMesh;
                var receiver = receivers[0];

                if (hit.collider.GetType() == typeof(MeshCollider))
                {
                    subMesh = ((MeshCollider)hit.collider).sharedMesh.GetSubMeshNumber(hit.triangleIndex);

                    if (receivers.Length > 1)
                    {
                        var mats = receiver.Renderer.materials;
                        var material = mats[subMesh % mats.Length];
                        receiver = receivers.FirstOrDefault(r => r.Material == material);
                    }
                }
                else
                    subMesh = receiver.materialIndex;

                if (!receiver) 
                    return;

                var tex = receiver.GetTexture();

                if (!tex) 
                    return;

                if (receiver.texture is Texture2D)
                {
                    if (hit.collider.GetType() != typeof(MeshCollider))
                        Debug.Log("Can't get UV coordinates from a Non-Mesh Collider");

                    BlitFunctions.Paint(receiver.useTexcoord2 ? hit.textureCoord2 : hit.textureCoord, 1, (Texture2D)receiver.texture, Vector2.zero, Vector2.one, brush);
                    var id = receiver.texture.GetTextureMeta();

                    return;
                }

                var rendTex = receiver.TryGetRenderTexture();

                if (rendTex)
                {
                    var st = new Stroke(hit, receiver.useTexcoord2);

                    st.unRepeatedUv = hit.collider.GetType() == typeof(MeshCollider)
                        ? (receiver.useTexcoord2 ? hit.textureCoord2 : hit.textureCoord).Floor()
                        : receiver.meshUvOffset;

                    if (receiver.type == C_PaintingReceiver.RendererType.Skinned && receiver.skinnedMeshRenderer)
                        BrushTypes.Sphere.Paint(
                            receiver.TryMakePaintCommand(st, brush, subMesh));

                    else if (receiver.type == C_PaintingReceiver.RendererType.Regular && receiver.meshFilter)
                    {
                        BrushTypes.Sphere.Paint(receiver.TryMakePaintCommand(st, brush, subMesh));
                   
                    } else 
                    {
                        QcLog.ChillLogger.LogErrorOnce("wasn't setip right for painting", "NoRtPntng" + name, this);
                    }

                }
                
            }
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) && TryHit(out var hit))
                Paint(hit.point);
        }

        #region Inspector
        private void OnDrawGizmosSelected()
        {
            if (TryHit(out RaycastHit hit))
            {
                var target = hit.transform.GetComponentInParent<C_PaintingReceiver>();
                Gizmos.color = target ? Color.green : Color.blue;
                Gizmos.DrawLine(transform.position, hit.point);
            }
        }

        public void Inspect()
        {
            brush.Nested_Inspect();
        }
        #endregion

    }

    [PEGI_Inspector_Override(typeof(C_PlayerShootingSource))]
    internal class C_PlayerShootingSourceDrawer : PEGI_Inspector_Override { }
}