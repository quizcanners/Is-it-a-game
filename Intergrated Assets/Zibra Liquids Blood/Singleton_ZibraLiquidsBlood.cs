using com.zibra.liquid.Manipulators;
using com.zibra.liquid.Solver;
using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.FinalIK.GrounderQuadruped;

namespace QuizCanners.IsItGame
{
    public class Singleton_ZibraLiquidsBlood : Singleton.BehaniourBase
    {
      
        [SerializeField] private ZibraLiquid _solver;
        [SerializeField] private List<SplashingEmitter> _emitters = new();
        [SerializeField] private ZibraLiquidForceField _explosionForceField;

        [SerializeField] private float _emission = 0.05f;

        private int _latestEmitter;

        [Serializable]
        private class SplashingEmitter : IPEGI_ListInspect
        {
            [SerializeField] private ZibraLiquidEmitter _emitter;
            [SerializeField] private ZibraLiquidForceField _forceField;
            [SerializeField] public Transform Root;

            Singleton_ZibraLiquidsBlood Mgmt => Singleton.Get<Singleton_ZibraLiquidsBlood>();
            public void Update() 
            {
                if (!Mgmt._debug)
                {
                    if (_emitter.VolumePerSimTime > 0)
                    {
                        _emitter.VolumePerSimTime = LerpUtils.LerpBySpeed(_emitter.VolumePerSimTime, 0, speed: 3 * Mgmt._emission, unscaledTime: false);
                    }
                }

                _forceField.transform.localPosition = UnityEngine.Random.insideUnitSphere * 0.5f;
            }

            public void Reset()
            {
                _emitter.VolumePerSimTime = Mgmt._debug ? Mgmt._emission : 0;
            }

            public void Emit(Vector3 position, float amount)
            {
                Root.transform.position = position;
                _emitter.VolumePerSimTime = Mgmt._emission * amount;
            }

            public void InspectInList(ref int edited, int index)
            {
                pegi.Edit(ref Root);
                pegi.Edit(ref _emitter);
                pegi.Edit(ref _forceField);
            }
        }

        private Gate.UnityTimeScaled _sinceLastBloodSpawn = new();
        private Gate.UnityTimeScaled _explosionPushDuration = new();

        private Vector3 targetPosition;

        private bool _debug;
        private bool _freezePosition;

        private bool Active
        {
            get => _solver.enabled;
            set => _solver.enabled = value;
        }

        public void TryCreateExplosion(Vector3 position) 
        {
           // if (IsInsideBounds(position))
         //   {
                _explosionForceField.transform.position = position;
                _explosionForceField.enabled = true;
                _explosionPushDuration.Update();
           // }
        }

        public bool TryEmitFrom(Vector3 source, Vector3 direction, float amountFraction = 1)
        {
            if (TryResetEmitionInternal(source, amountFraction, out var emitter))
            {
                emitter.Root.transform.LookAt(source + direction);
                return true;
            }

            return false;
        }

        public bool TryEmitFrom(Transform source, Vector3 direction, float amountFraction = 1)
        {
            if (TryResetEmitionInternal(source.position, amountFraction, out var emitter))
            {
                emitter.Root.transform.LookAt(source.position + source.TransformDirection(direction));
                return true;
            }

            return false;
        }

        public bool TryEmitFrom(RaycastHit hit, float amountFraction = 1) 
        {
            if (TryResetEmitionInternal(hit.point, amountFraction, out var emitter))
            {
                emitter.Root.transform.LookAt(hit.point + hit.normal, Vector3.up);
                return true;
            }

            return false;
          
        }

        private bool IsInsideBounds(Vector3 point) 
        {
            var fromCenter = (point - transform.position).Abs();

            var size = _solver.containerSize;

            return fromCenter.x < size.x * 0.2f && fromCenter.z < size.z * 0.2f;
        }

        public Vector3 Size => _solver.containerSize;

        private bool TryResetEmitionInternal(Vector3 newPosition, float amountFraction, out SplashingEmitter emitter) 
        {
            if (!IsInsideBounds(newPosition)) //fromCenter.x > size.x * 0.2f || fromCenter.z > size.z * 0.2f)
            {
                if (_freezePosition)
                {
                    emitter = null;
                    return false;
                }

                if (_sinceLastBloodSpawn.TryUpdateIfTimePassed(3f))
                {
                    transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
                    targetPosition = transform.position;
                    //_solver.UpdateSimulation(0.001f);
                } else 
                {
                    var fromCenter = (newPosition - transform.position).Abs();
                    var size = Size;

                    if (fromCenter.x < size.x * 0.25f && fromCenter.z < size.z * 0.25f)
                    {
                        targetPosition = newPosition;
                        targetPosition.y = transform.position.y;
                    }
                    else
                    {
                        emitter = null;
                        return false;
                    }
                }
            }

            Active = true;

         

            _latestEmitter = (_latestEmitter + 1) % _emitters.Count;

            emitter = _emitters[_latestEmitter];

            emitter.Emit(newPosition, amountFraction);

            _sinceLastBloodSpawn.Update();

            return true;
        }

        protected override void OnAfterEnable()
        {
            base.OnAfterEnable();

            foreach (var s in _emitters)
                s.Reset();

            targetPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (Active && _freezePosition == false)
            {
               transform.position = LerpUtils.LerpBySpeed(transform.position, targetPosition, 3, unscaledTime: true);
            }
        }


        private readonly Gate.UnityTimeUnScaled _shapesUpdateGate = new();

        private void Update()
        {
            //  _forceField.Strength = Mathf.Clamp(_forceField.Strength + (Random.value - 0.5f)*3, -2, 2);

            if (!Active)
                return;

            if (_explosionForceField.enabled && _explosionPushDuration.TryUpdateIfTimePassed(0.3f))
                _explosionForceField.enabled = false;

            if (!_debug && _sinceLastBloodSpawn.TryUpdateIfTimePassed(5))
            {
                Active = false;
            }
            
            foreach (var s in _emitters) 
            {
                s.Update();
            }

            if (_shapesUpdateGate.TryUpdateIfTimePassed(1f))
            {
                var colliders = _solver.GetColliderList();

                Singleton.Try<Singleton_EnvironmentElementsManager>(s =>
                {
                    var sorted = s.GetSortedForBox(_solver.transform.position, Size, QcRTX.Shape.Cube);

                    for (int i = 0; i < colliders.Count; i++)
                    {
                        var col = colliders[i];
                        if (sorted.Count > i)
                        {
                            var el = sorted[i].EnvironmentElement;
                            col.transform.SetPositionAndRotation(el.PrimitiveCenter, el.transform.rotation);

                            Vector3 pSize = el.PrimitiveSize;
                            pSize.Scale(el.transform.localScale);
                            col.transform.localScale = el.PrimitiveSize;
                        }
                        else HideCollider(col);
                    }
                }
                , onFailed: () =>
                {
                    // If no colliders to set
                    foreach (ZibraLiquidCollider c in colliders)
                    {
                        HideCollider(c);
                    }
                });

                void HideCollider(ZibraLiquidCollider c)
                {
                    c.transform.position = Vector3.down * 10;
                    c.transform.localScale = Vector3.one;
                }
            }
          
        }

        private readonly pegi.CollectionInspectorMeta _emittersMeta = new("Emitters");

        public override void Inspect()
        {
            base.Inspect();
            pegi.Nl();

            if ("Debug".PegiLabel().ToggleIcon(ref _debug).Nl() && _debug) 
            {
                foreach (var s in _emitters)
                    s.Reset();
                _solver.enabled = true;
            }

            "Freeze Position".PegiLabel().ToggleIcon(ref _freezePosition).Nl();

            if (_debug) 
            {
                if ("Rapidly change position".PegiLabel().Click().Nl()) 
                {
                    transform.position += new Vector3(UnityEngine.Random.Range(-5, 5), 0, UnityEngine.Random.Range(-5, 5));
                }
            }

            "Solver".PegiLabel().Edit_IfNull(ref _solver, gameObject).Nl();

            _emittersMeta.Edit_List(_emitters).Nl();
        }

    }

    [PEGI_Inspector_Override(typeof(Singleton_ZibraLiquidsBlood))] internal class ZibraLiquidSingletonManagerDrawer : PEGI_Inspector_Override { }

}