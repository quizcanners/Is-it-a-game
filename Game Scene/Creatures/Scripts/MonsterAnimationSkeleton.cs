using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public partial class C_MonsterEnemy 
    {
        [Serializable]
        protected class AnimatedSkeleton : IPEGI, INeedAttention
        {
            [SerializeField] public Animator animator;
            readonly AnimatorValue.Float SPEED = new("Speed");
            readonly AnimatorValue.Trigger DIE = new("Die");
            readonly AnimatorValue.Bool DOWN = new("Dead");
            readonly AnimatorValue.Trigger ATTACK = new("Attack");
            readonly AnimatorValue.Trigger HIT = new("Hit");
            readonly AnimatorValue.Trigger BLOCK = new("Block");

            Vector3 _previousPosition;
            Vector3 _directionVector;
            private List<AnimatorValue.Base> _bases;

            private Gate.UnityTimeScaled _hitReactionDelay = new(Gate.InitialValue.StartArmed);

            public Vector3 Direction => _directionVector;

            protected List<AnimatorValue.Base> AllValues
            {
                get
                {
                    if (_bases.IsNullOrEmpty())
                    {

                        _bases = new List<AnimatorValue.Base>() {
                        ATTACK,
                        SPEED,
                        DIE,
                        DOWN,
                        HIT,
                        BLOCK,
                    };
                    }
                    return _bases;
                }
            }

            public void OnMove(Vector3 position)
            {
                var newDirection = (position - _previousPosition);
                newDirection.y =0;
               
                _directionVector = Vector3.Lerp(_directionVector, newDirection.normalized, Time.deltaTime * 10);
                _directionVector.y = 0;

                float speed = newDirection.magnitude / Time.deltaTime;
                _previousPosition = position;
                SPEED.SetOn(speed, animator);
            }

            public void Freeze() => animator.speed = 0.15f;

            public void ResetAnimator(Vector3 position)
            {
                _previousPosition = position;
                animator.speed = 1f;
                AllValues.Reset(animator);
            }

            public void PlayBlockAnimation()
            {
                if (_hitReactionDelay.TryUpdateIfTimePassed(0.5f))
                    Play(BLOCK);
            }

            public void GetHit()
            {
                if (_hitReactionDelay.TryUpdateIfTimePassed(0.5f))
                    Play(HIT);
            }
            public bool Down
            {
                get => DOWN.Get();
                set
                {
                    if (DOWN.SetOn(value, animator) && value)
                        Play(DIE);
                }
            }

            private void Play(AnimatorValue.Trigger trig) => trig.SetOn(animator);

            public void Inspect()
            {
                if (!animator)
                    pegi.Edit(ref animator).Nl();

                if (animator)
                {
                    "Kill".PegiLabel().Click(() => Down = true).Nl();
                    SPEED.Inspect(animator); pegi.Nl();
                    ATTACK.Inspect(animator); pegi.Nl();
                    HIT.Inspect(animator); pegi.Nl();
                    BLOCK.Inspect(animator); pegi.Nl();
                }
            }

            public string NeedAttention()
            {
                if (!animator)
                    return "{0} is not assigned".F(nameof(animator));

                return null;
            }
        }

    }
}
