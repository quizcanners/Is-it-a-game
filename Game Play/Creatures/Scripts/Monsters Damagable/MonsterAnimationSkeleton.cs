using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    [Serializable]
    public class Monster_AnimatedSkeleton : IPEGI, INeedAttention
    {
        [SerializeField] public Animator animator;


        private readonly AnimatorValue.Float SPEED = new("Speed");
        private readonly AnimatorValue.Trigger DIE = new("Die");
        private readonly AnimatorValue.Bool DOWN = new("Dead");
        private readonly AnimatorValue.Trigger ATTACK = new("Attack");
        private readonly AnimatorValue.Trigger HIT = new("Hit");
        private readonly AnimatorValue.Trigger BLOCK = new("Block");

        private readonly CreatureAnimator_Speed _speedTracking = new();

           
        private List<AnimatorValue.Base> _bases;

        private readonly Gate.UnityTimeScaled _hitReactionDelay = new(Gate.InitialValue.StartArmed);

        public Vector3 Direction => _speedTracking.DirectionVector;

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
            _speedTracking.OnMove(position, out float speed);
            animator.Set(SPEED, speed);
        }

        public void Freeze() => animator.speed = 0.15f;

        public void ResetAnimator(Vector3 position)
        {
            _speedTracking.Reset(position);
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

        private void Play(AnimatorValue.Trigger trig) => animator.Set(trig); 

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
