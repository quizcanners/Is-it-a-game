using Dungeons_and_Dragons;
using PainterTool;
using QuizCanners.Inspect;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayerGun_BoltGun : IPEGI, INeedAttention
    {
        public PlaytimePainter_BrushConfigScriptableObject brushConfig;
        public Attack WeaponAttack = new(name: "Gun", isRange: true, attackBonus: 3,
             new Damage()
             {
                 DamageBonus = 2,
                 DamageDice = new List<Dice> { Dice.D10 },
                 DamageType = DamageType.Piercing
             });

        public void Shoot(Vector3 from, Vector3 target)
        {





        }

        #region Inspector

        [SerializeField] private pegi.EnterExitContext context = new();

        public void Inspect()
        {
            using (context.StartContext())
            {
                "Brush Config".PegiLabel().Edit_Enter_Inspect(ref brushConfig).Nl();
                WeaponAttack.Enter_Inspect_AsList().Nl();
            }
        }

        public virtual string NeedAttention()
        {
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