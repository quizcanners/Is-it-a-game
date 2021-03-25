using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Monster : Creature, IGotReadOnlyName
    {
        public ChallangeRating ChallengeRating;

        public CreatureType Type;

        [SerializeField] private Size _size = Size.Medium;
        [SerializeField] protected CreatureSpeed Speed = new();
        [SerializeField] private Fallback.Int _hitDices_Fallback = new();
        [SerializeField] public List<Weapon.SmartId> Weapons = new();
        [SerializeField] private Fallback.Int _naturalArmor = new();

        public int HitDices 
        {
            get => _hitDices_Fallback[GetFallbackHitDiceCount];
            set => _hitDices_Fallback.ManualValue = value;
        }

        public override int MaxHitPoints => Mathf.Max(1, Size.GetHitDice().AvargeRoll(diceCount: HitDices) + this[AbilityScore.Constitution] * HitDices);

        private int GetFallbackHitDiceCount() => Math.Max(1, Mathf.FloorToInt(ChallengeRating.ExpectedMaxHp() / (_size.GetHitDice().AvargeRoll() + 0.5f + this[AbilityScore.Constitution])));

        public override Size Size => _size;

        public override int ArmorClass => _naturalArmor.Get(defaultValue: ChallengeRating.ExpectedArmorClass());

        protected override int ProficiencyBonus => ChallengeRating.ProficiencyBonus();

        public override GridDistance this[SpeedType type] => Speed.TryGetValue(type, out GridDistance dist) ? dist : GridDistance.FromCells(0);

        protected override Proficiency GetProficiency(Skill stat) => base.GetProficiency(stat);

        protected override Proficiency SavingThrowProficiency(AbilityScore stat) => base.SavingThrowProficiency(stat);

        protected override int GetAbilityScoreRacialBonus(AbilityScore stat) => 0;

        public override List<Attack> GetAttacks() 
        {
            var lst = new List<Attack>();
            foreach (var w in Weapons)
            {
                if (TryGetAttack(w, out var attack))
                    lst.Add(attack);
            }

            return lst;
        }

        #region Inspector
        protected override void Inspect_HeaderBlock()
        {
            base.Inspect_HeaderBlock();

            "{0} {1}, {2}".F(_size, Type, Allignment.GetReadOnlyName()).PegiLabel().nl();
        }

        protected override void Inspect_AcHpSpeedBlock()
        {
            "Armor Class {0}{1}".F(ArmorClass,  _naturalArmor.Get(0) > 0 ? "(natural armor)" : "").PegiLabel().nl();
            "Hit Points {0}".F(MaxHitPoints).PegiLabel().nl();
            "Speed {0}".F(this[SpeedType.Walking]).PegiLabel().nl();

        }


        private int _inspectedWeapon = -1;

        protected override void Inspect_Contextual()
        {
            if (enterExitContext.IsAnyEntered == false)
            {
                "Size".PegiLabel().editEnum(ref _size).nl();
                pegi.editEnum(ref Type);
                pegi.nl();

                _naturalArmor.Inspect("Armor Class", ChallengeRating.ExpectedArmorClass()); pegi.nl();

                _hitDices_Fallback.Inspect("hit Dices", GetFallbackHitDiceCount()); pegi.nl();

                "Challange Rating".PegiLabel(120).editEnum(ref ChallengeRating, cr => cr.GetReadableString());

                if (ChallengeRating > ChallangeRating.CR_0)
                    icon.Down.Click(() => ChallengeRating = (ChallangeRating)((int)ChallengeRating - 1));
                if (ChallengeRating < ChallangeRating.CR_30)
                    icon.Up.Click(() => ChallengeRating = (ChallangeRating)((int)ChallengeRating + 1));
                pegi.nl();

                int avgRoll = _size.GetHitDice().AvargeRoll(HitDices);
                int bonus = this[AbilityScore.Constitution] * HitDices;

                "Hp: {0} ({1} {2}) / ({3}-{4})".F(avgRoll + bonus, "{0}d{1}".F(HitDices, (int)Size.GetHitDice()), bonus.ToSignedNumber(), ChallengeRating.ExpectedMinHp(), ChallengeRating.ExpectedMaxHp()).PegiLabel().write();

                if (_hitDices_Fallback.IsSet)
                    icon.Refresh.Click(() => HitDices = GetFallbackHitDiceCount());       
                    
                pegi.nl();
            }
            
            "Speed".PegiLabel().enter_Inspect(Speed).nl();

            "Weapons".F(Weapons.Count).PegiLabel().enter_List(Weapons, ref _inspectedWeapon).nl();

            if (enterExitContext.IsCurrentEntered) 
            {
                "Expected Attack Bonus: {0}".F(ChallengeRating.ExpectedAttackBonus()).PegiLabel().nl();
                Inspect_Actions(GetAttacks());
            }
        }

        internal override int GetDefaultValue(AbilityScore stat) => 10;

        public string GetReadOnlyName() => "{0} (CR {1})".F(NameForInspector, ChallengeRating.GetReadableString());

        public override IEnumerator<object> SearchKeywordsEnumerator()
        {
            yield return _size.ToString();

            yield return Type.ToString();

            foreach (var w in Weapons)
                yield return w;

            yield return base.SearchKeywordsEnumerator();
        }

        #endregion

        [Serializable]
        public class SmartId : DnD_SmartId<Monster>
        {
            protected override Dictionary<string, Monster> GetEnities() => Data.Monsters;
        }
    }


   

    [Serializable]
    public class MonstersDictionary : SerializableDictionary<string, Monster> { }


    [Serializable]
    public class CreatureSpeed: SerializableDictionary_ForEnum<SpeedType, GridDistance> 
    {
        public override void Create(SpeedType key)
        {
            this[key] = GridDistance.FromCells(6);
        }

        public CreatureSpeed() 
        {
            Add(SpeedType.Walking,  GridDistance.FromCells(6));
        }
    }


 
}
