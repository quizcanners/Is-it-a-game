using GPUInstancer.CrowdAnimations;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterInstanced : CreatureProxy_Base
    {
       // [SerializeField] protected Monster_AnimatedSkeleton skeleton;
        [SerializeField] protected GPUICrowdPrefab instance;
        [SerializeField] protected AnimationClip spawnClip;
        [SerializeField] protected AnimationClip walkingClip;
        [SerializeField] protected AnimationClip runningClip;
        [SerializeField] protected AnimationClip deathClip;
        [SerializeField] protected AnimationClip idleClip;
        [SerializeField] protected AnimationClip impactClip;

        private State state;

        Vector3 GetActivePosition() => transform.position + Vector3.up *0.5f;


        public void OnHit(bool damage) 
        {
            /*if (damage)
                skeleton.GetHit();
            else
                skeleton.PlayBlockAnimation();*/

            Play(impactClip);
            state = State.IsHit;
        }

        public override void Giblets(Vector3 pushVector = default, float pushForce01 = 0)
        {
            if (state == State.Disintegrated)
                return;

            state = State.Disintegrated;

            IsAlive = false;

            Vector3 origin = GetActivePosition();

            if (Camera.main.IsInCameraViewArea(origin))
            {
                Game.Enums.UiSoundEffects.BodyExplosion.PlayOneShotAt(origin);

                if (Camera.main.IsInCameraViewArea(origin, maxDistance: 50))
                {
                    Monster_Gore_Utils.SprayBloodParticles(origin: origin, pushVector: pushVector, pushForce01: pushForce01);
                }
                else
                {
                    Monster_Gore_Utils.PaintBigBloodSplatter(origin: origin, pushVector: pushVector, pushForce01: pushForce01);
                }

                Singleton.Try<Pool_VolumetricBlood>(s =>
                {
                    for (int i = 0; i < 2; i++)
                        if (!s.TrySpawnRandom(origin, pushVector, out var inst, size: 2.5f))
                            break;
                });
            }

            Pool.Return(this);
        }


        public override void Connect(C_Monster_Data data)
        {
            base.Connect(data);
           // skeleton.ResetAnimator(transform.position);
            Update();
            state = State.Unspawned;
        }

        public override void ApplyImpact(RaycastHit hit, ref bool pierced, Action<Vector3> onDismemberment)
        {
            if (Parent)
            {
                if (Camera.main.IsInCameraViewArea(hit.point))
                {
                    var oldParent = Parent;
                    if (oldParent.TrySetProxy(C_Monster_Data.Proxy.Detailed))
                    {
                        oldParent.proxy_detailed.ApplyImpact(hit, ref pierced, onDismemberment);
                    }
                }
            }
        }

        private void Update()
        {
            if (Parent)
            {
                var pTf = Parent.transform;
                transform.SetPositionAndRotation(pTf.position, pTf.rotation); 
            }

       //     skeleton.Down = !IsAlive;

        //    skeleton.OnMove(transform.position);

          //  GPUInstancer.CrowdAnimations.GPUICrowdAPI.StartAnimation(instance, walkingClip);
        }

        public override void Disconnect()
        {
            base.Disconnect();

            //Giblets();

            Singleton.Try<Pool_MonstersGPU_Instanced>(s =>
            {
                s.ReturnToPool(this);
            });
        }

        void OnDisable() 
        {
            Disconnect();
        }

        private enum State 
        {
            Unspawned, WaitToWalk, Walking, IsHit, Disintegrated
        }

        private Gate.UnityTimeScaled _animationTime = new();

        private void LateUpdate()
        {
            if (instance.crowdAnimator == null)
                return;

            switch (state) 
            {
                case State.Unspawned: Play(spawnClip); state = State.WaitToWalk; break;
                case State.WaitToWalk:
                    if (_animationTime.TryUpdateIfTimePassed(1))
                    {
                        Play(walkingClip);
                        state = State.Walking;
                    }
                    break;
                case State.IsHit:
                    if (_animationTime.TryUpdateIfTimePassed(1))
                    {
                        Play(walkingClip);
                        state = State.Walking;
                    }
                    break;
            }

           

        }



        void Play(AnimationClip clip) 
        {
            if (!instance)
                Debug.LogError("No instance");

            if (!clip)
                Debug.LogError("No clip");

            instance.StartAnimation(clip);

            _animationTime.Update();
          //  instance.crowdAnimator.StartBlend()
        }

        #region Inspector
        public override void Inspect()
        {
            //skeleton.Nested_Inspect().Nl();

            pegi.Nl ();

            Test("spawnClip", ref spawnClip);
            Test("walkingClip", ref walkingClip);
            Test("runningClip", ref runningClip);
            Test("deathClip", ref deathClip);
            Test("idleClip", ref idleClip);
            Test("impactClip", ref impactClip);

            void Test(string cname, ref AnimationClip clip) 
            {
                cname.PegiLabel().Edit(ref clip);
                if (clip && Icon.Play.Click())
                    Play(clip);

                pegi.Nl();
            }

        }

        #endregion

    }

    [PEGI_Inspector_Override(typeof(C_MonsterInstanced))] internal class C_MonsterInstancedDrawer : PEGI_Inspector_Override { }
}