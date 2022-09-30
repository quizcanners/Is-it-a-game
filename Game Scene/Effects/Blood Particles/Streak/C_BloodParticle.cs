using PainterTool;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [DisallowMultipleComponent]
    public class C_BloodParticle : MonoBehaviour, IPEGI
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private Collider _collider;

        [NonSerialized] private LogicWrappers.Timer _maxLifetime = new();
        [NonSerialized] private LogicWrappers.Timer _afterPaintTimer = new();

        private bool _painted = false;

        void OnTriggerEnter(Collider other)
        {

            if (_painted)
                return;

            var receiver = other.transform.gameObject.GetComponent<C_PaintingReceiver>();

            if (receiver)
            {
                var brush = Singleton.TryGetValue<Pool_BloodParticlesController, Brush>(s => s.Brush);

                if (brush != null) 
                {
                    var rendTex = receiver.GetTexture() as RenderTexture;

                    if (rendTex)
                    {
                        var vec = _rigidbody.velocity.normalized;
                        var st = new Stroke(transform.position - vec, _rigidbody.velocity + vec);
                        receiver.CreatePaintCommandFor(st, brush, 0).Paint();
                    }
                }

                _collider.enabled = false;
                _painted = true;
                _afterPaintTimer.Restart(0.5f);

                return;
            }
        }

        void Update() 
        {
            /*if (LerpUtils.IsLerpingBySpeed(ref bloodOffset, 0, 1)) 
            {
                _trailRenderer.transform.position = transform.position - _rigidbody.velocity.normalized * bloodOffset;
            }*/

            if ((_painted && _afterPaintTimer.IsFinished) || _maxLifetime.IsFinished) 
            {
                Pool.Return(this);
            }
        }

        public void Restart(Vector3 position, Vector3 direction, float scale) 
        {
            float size = 0.1f + UnityEngine.Random.value * 0.9f * scale;

            _rigidbody.drag = 0.1f + size * 0.4f;
            transform.position = position;
            _rigidbody.velocity = direction * size * 2;
            _maxLifetime.Restart(5);
            _trailRenderer.Clear();
            _trailRenderer.widthMultiplier = size;
            _painted = false;
            _collider.enabled = true;
        }

        #region Inspector
        public void Inspect()
        {
            "Collider".PegiLabel(60).Edit_IfNull(ref _collider, gameObject).Nl();
            "Trail".PegiLabel(60).Edit_IfNull(ref _trailRenderer, gameObject).Nl();
            "Rigidbody".PegiLabel().Edit_IfNull(ref _rigidbody, gameObject).Nl();
            "Spawn at 0 ".PegiLabel().Click().OnChanged(() => Restart(Vector3.zero + Vector3.up, Vector3.up, scale: 1)).Nl();
         //   "Contatct point".PegiLabel().Edit_List(latestCollisionDebug).Nl();
        }

        private class CollisionDebug : IPEGI_ListInspect
        {
            private readonly ContactPoint _point;

            internal CollisionDebug(ContactPoint point) 
            {
                _point = point;
            }

            public override string ToString() => _point.otherCollider.gameObject.name;

            public void InspectInList(ref int edited, int index)
            {
                pegi.ClickHighlight(_point.otherCollider.gameObject);
                _point.otherCollider.gameObject.name.PegiLabel().Write();
            }
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_BloodParticle))] internal class C_BloodParticleDrawer : PEGI_Inspector_Override { }
}