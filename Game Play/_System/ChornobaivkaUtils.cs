using Dungeons_and_Dragons;
using PainterTool.Examples;
using PainterTool;
using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public static class ChornobaivkaUtils 
    {
        public static void PaintDamageOnImpact_IfVisible(RaycastHit hit, Brush brush, Vector3 direction, bool dismemberment, Vector3 damagePosition) 
        {
            if (Camera.main.IsInCameraViewArea(hit.point))
            {
                var receivers = hit.transform.GetComponentsInParent<C_PaintingReceiver>();

                if (receivers.Length > 0)
                {
                    C_PaintingReceiver receiver = receivers.GetByHit(hit, out int subMesh);

                    if (receiver)
                    {
                        var tex = receiver.GetTexture();

                        if (tex)
                        {
                            if (tex is Texture2D d)
                            {
                                if (hit.collider.GetType() != typeof(MeshCollider))
                                    Debug.Log("Can't get UV coordinates from a Non-Mesh Collider");

                                BlitFunctions.Paint(receiver.UseTexcoord2 ? hit.textureCoord2 : hit.textureCoord, 1, d, Vector2.zero, Vector2.one, brush);
                                var id = tex.GetTextureMeta();

                                return;
                            }

                            var rendTex = tex as RenderTexture;

                            if (rendTex)
                            {
                                float wallShootTrough = 0.5f;

                                var hitVector = direction;//(hit.point - from).normalized;

                                var st = receiver.CreateStroke(hit, hitVector.normalized * (wallShootTrough + 0.2f));

                          
                                if (dismemberment)
                                {
                                    st.posFrom = damagePosition;
                                    st.posTo = damagePosition;

                                    Singleton.Try<Singleton_ChornobaivkaController>(s => brush = s._config.DismembermentBrush.brush);
                                }

                                st.posFrom -= hitVector * 0.2f;

                                receiver.CreatePaintCommandFor(st, brush, subMesh).Paint();
                            }
                        }

                    }
                }
            }

        }

    }
}