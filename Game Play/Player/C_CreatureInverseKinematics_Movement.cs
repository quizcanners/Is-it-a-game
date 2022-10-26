using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_CreatureInverseKinematics_Movement : MonoBehaviour, IPEGI
    {

        [SerializeField] private Animator _animator;
        [SerializeField] private RectTransform _taret;

        
       // private readonly AnimatorValue.Float Speed = new("Speed");

        private sealed class AIM
        {
            private static readonly AnimatorValue.Float FORWARD = new("Aim Forward");
            private static readonly AnimatorValue.Float LEFT = new("Aim Left");
            private static readonly AnimatorValue.Float RIGHT = new("Aim Right");
            private static readonly AnimatorValue.Float UP = new("Aim Up");
            private static readonly AnimatorValue.Float DOWN = new("Aim Down");
        }

        private readonly CreatureAnimator_Speed _speedTracking = new();

        void OnEnable() 
        {
          //  _speedTracking.Reset(transform.position);
        }

        void LateUpdate()
        {
           // _speedTracking.OnMove(transform.position, out float speed);
           // _animator.Set(Speed, speed);
        }

        public void Inspect()
        {
            "Animator".PegiLabel(70).Edit_IfNull(ref _animator, gameObject).Nl();
        }
    }

    [PEGI_Inspector_Override(typeof(C_CreatureInverseKinematics_Movement))] internal class C_PlayerInverseKinematics_MovementDrawer : PEGI_Inspector_Override { }
}