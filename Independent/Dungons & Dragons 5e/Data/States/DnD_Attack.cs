using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Attack : IPEGI_ListInspect, IGotReadOnlyName
    {
        public string Name;
        public int AttackBonus;
        public FeetDistance range = new FeetDistance() { ft = 5 };
        public List<Dice> DamageDice = new List<Dice>();
        public DamageType DamageType;
        public bool IsRange;
        public int DamageBonus;


        public int AvarageDamage => DamageDice.AvargeRoll() + DamageBonus;

        public bool RollAttack(RollInfluence influence, int armorClass, out bool isCriticalHit)
        {
            var roll = Dice.D20.Roll(influence).Value;
            isCriticalHit = (roll == 20);

            if (isCriticalHit)
                return true;

            if (roll == 1)
                return false;

            return (roll + AttackBonus)>= armorClass;
        }

        public int RollDamage(bool isCritical) => DamageDice.Roll().Value + (isCritical ? DamageDice.Roll().Value : 0) + DamageBonus; 
        
        public Attack (string name, bool isRange, int attackBonus, List<Dice> damageDice,  int damageBonus, DamageType damageType) 
        {
            Name = name;
            AttackBonus = attackBonus;
            DamageDice = damageDice;
            IsRange = isRange;
            DamageBonus = damageBonus;
            DamageType = damageType;
        }

        public void InspectInList(ref int edited, int index)
        {
            "+".PegiLabel(15).edit(ref AttackBonus, 20);

            "To Hit, ".PegiLabel(40).write();
            if (DamageDice.Count == 0)
            {
                "+Dice".PegiLabel().Click(() => DamageDice.Add(Dice.D6));
                "Dmg".PegiLabel(30).edit(ref DamageBonus, 30);
            }
            else
            {
                int cnt = DamageDice.CountOccurances(DamageDice[0]);
                var first = DamageDice[0];

                var changed = pegi.ChangeTrackStart();

                pegi.editDelayed(ref cnt, 25);
                "d".PegiLabel(15).editEnum(ref first, d => ((int)d).ToString(), 35);

                if (changed)
                {
                    DamageDice.Clear();
                    for (int i = 0; i < cnt; i++)
                        DamageDice.Add(first);
                }
                "+".PegiLabel(15).edit(ref DamageBonus, 25);
                "dmg.".PegiLabel(35).write();
            }
        }

        public string GetReadOnlyName()
        {
            return "{0}. {1} Weapon Attack: +{2} to hit, reach {3}, one target. Hit: {4} {5} damage.".
                F(Name, IsRange ? "Range" : "Melee", AttackBonus, range.ToString(), DamageDice.ToDescription(DamageBonus), DamageType);
        }
    }
}
