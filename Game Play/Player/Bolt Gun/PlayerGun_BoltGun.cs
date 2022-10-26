using Dungeons_and_Dragons;
using PainterTool;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayerGun_BoltGun : IPEGI, INeedAttention
    {
        [SerializeField] private float speed = 1;
        public PlaytimePainter_BrushConfigScriptableObject brushConfig;
        public Attack WeaponAttack = new(name: "Gun", isRange: true, attackBonus: 3,
             new Damage()
             {
                 DamageBonus = 2,
                 DamageDice = new List<Dice> { Dice.D10 },
                 DamageType = DamageType.Piercing
             });


        public void Shoot(Vector3 from, Vector3 target, State state)
        {
            Singleton.Try<Pool_BoltProjectiles>(s => 
            {
                Game.Enums.UiSoundEffects.BoltGun_Shoot.PlayOneShotAt(from, clipVolume: 3);

                s.TrySpawn(from, instance => 
                {
                    var vector = target - from;

                    float distance = vector.magnitude;

                    float flyTime =  distance / speed;

                    Vector3 gravityCompensation = -Physics.gravity * 0.5f * flyTime;

                    var velocity = gravityCompensation + vector.normalized * speed;

                    instance.Restart(velocity, this);
                });
            });

        }


        [Serializable]
        public class State : IPEGI
        {
            public readonly Gate.UnityTimeScaled DelayBetweenShots = new(Gate.InitialValue.StartArmed);

            public void Inspect()
            {

            }
        }

        #region Inspector

        public override string ToString() => "Bolt Gun";

        [SerializeField] private pegi.EnterExitContext _context = new();

        public void Inspect()
        {
            using (_context.StartContext())
            {
                if (_context.IsAnyEntered == false) 
                {
                    "Bolt Speed".PegiLabel().Edit(ref speed).Nl();
                }

                "Brush Config".PegiLabel().Edit_Enter_Inspect(ref brushConfig).Nl();
                WeaponAttack.Enter_Inspect_AsList().Nl();
            }
        }

        public virtual string NeedAttention()
        {
            if (speed <= 0)
                return "Speed is zero";

            if (!brushConfig)
                return "Assign Brush Config";

            var brush = brushConfig.brush;

            if (!brush.Is3DBrush())
                return "Should be a Sphere Brush";

            if (brush.FallbackTarget != TexTarget.RenderTexture)
                return "Should render to Render Texture";

            return null;
        }

        #endregion
    }
}