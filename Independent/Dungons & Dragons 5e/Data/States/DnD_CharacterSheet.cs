using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class CharacterSheet : Creature, IGotReadOnlyName
    {
        [SerializeField] private bool _nameDecided;
        [SerializeField] protected bool _manualName;
        [SerializeField] public CharacterLevel Level = new();
        [SerializeField] private RaceFallback _raceEnumFallback = new();
        [SerializeField] private ClassFallback _classEnumFallback = new();
        [SerializeField] internal protected int subRace;
        [SerializeField] internal protected Gender gender;
        [SerializeField] internal protected int subClass;
        [SerializeField] internal ArmorProficiencies ArmorProficiencies = new();
        [SerializeField] internal WeaponProficiencies WeaponProficiencies = new();
        [SerializeField] internal ToolProficiencies ToolProficiencies = new();
        [SerializeField] public Wallet Wallet = new();
        [SerializeField] public FeatsSet Feats = new();

        #region Seeded Fallbacks 

        protected Race RaceEnum 
        {
            get =>  _raceEnumFallback[
                    defaultValueGetter: ()=> Singleton.TryGetValue<DnD_Service, Race>(valueGetter: s => s.Fallbacks.GetRace(this),
                    defaultValue: Dungeons_and_Dragons.Race.Human)];
            set => _raceEnumFallback.ManualValue = value;
        }

        protected Class ClassEnum
        {
            get => _classEnumFallback[
                    defaultValueGetter: () => Singleton.TryGetValue<DnD_Service, Class>(valueGetter: s => s.Fallbacks.GetClass(this),
                    defaultValue: Dungeons_and_Dragons.Class.Fighter)];
            set => _classEnumFallback.ManualValue = value;
        }

        [Serializable]
        private class RaceFallback : Fallback.FallbackValueGeneric<Race> { }

        [Serializable]
        private class ClassFallback : Fallback.FallbackValueGeneric<Class> { }

        public CharacterRace Race => RaceEnum.Get();


        public override string NameForInspector
        {
            get => _manualName ? _name : Singleton.TryGetValue<DnD_Service, string>(s => s.Fallbacks.GetName(this, _name));
            set
            {
                _name = value;
                _manualName = true;
            }
        }

        #endregion


        public override bool TryGetConcept<T>(out T value) 
        {
            if (typeof(T) == typeof(Gender)) 
            {
                value = (T)((object)gender);
                return true;
            }

            if (typeof(T) == typeof(Race))
            {
                value = (T)((object)RaceEnum);
                return true;
            }

            return base.TryGetConcept(out value);
        }

        internal override int GetDefaultValue(AbilityScore stat) => Class.GetStandardArrayBaseScoreFor(stat);

        protected override int ProficiencyBonus => Level.ProficiencyBonus;

        public CharacterClass Class => ClassEnum.Get();


        public override bool this[Trait trait] => base[trait] || Race[trait];

        public bool this[Feature feature] => Class.HasFeature(feature, classLevel: Level, subClass: subClass);

        public override GridDistance this[SpeedType type] =>
            type switch
            {
                SpeedType.Walking => Race.WalkingSpeed,
                SpeedType.Swimming => Race.WalkingSpeed.Half(),
                SpeedType.Climbing => this[SpeedType.Walking].Half(),
                _ => GridDistance.FromCells(0),
            };

        public override Size Size => Size.Medium;

        public override int ArmorClass => 10 + this[AbilityScore.Dexterity] + (this[Feature.UnarmoredDefense] ? this[AbilityScore.Constitution] : 0);

        public override int MaxHitPoints
        {
            get
            {
                var hitDice = Class.HitDie;
                var mod = this[AbilityScore.Constitution];

                return hitDice.MaxRoll() + hitDice.AvargeRoll() * (Level - 1) + mod * Level;
            }
        }

        private Attack GetUnarmedStrike()
            => new("Unarmed Srike", isRange: false, attackBonus: ProficiencyBonus + this[AbilityScore.Strength], new List<Dice>(), damageBonus: 1 + this[AbilityScore.Strength], damageType: DamageType.Bludgeoning);

        public override List<Attack> GetAttacks()
        {
            var lst = new List<Attack>
            {
                GetUnarmedStrike()
            };
            return lst;
        }

        protected override Proficiency SavingThrowProficiency(AbilityScore stat) => Race[stat].And(Class[stat]).And(base.SavingThrowProficiency(stat));

        protected override Proficiency GetProficiency(Skill skill)
        {
            Proficiency prof = Race[skill].And(base.GetProficiency(skill));

            return prof;
        }

        protected override int GetAbilityScoreRacialBonus(AbilityScore stat) => Race.AbilityScoreRacialBonus(stat, subRace);

        #region Inspector

        public static CharacterSheet inspected;

        private readonly pegi.EnterExitContext _otherProfiiancies = new();

        protected override void Inspect_HeaderBlock()
        {
            GetReadOnlyName().PegiLabel(style: pegi.Styles.HeaderText).nl();
        }

        protected override void Inspect_Contextual()
        {
            inspected = this;

            if (enterExitContext.IsAnyEntered == false) 
            {
                var r = RaceEnum;
                "Race".PegiLabel(50).editEnum(ref r).OnChanged(()=> RaceEnum = r);

                if (Race.TryGetSubraces(out var sub))
                    sub.Inspect(ref subRace);

                pegi.nl();

                var cl = ClassEnum;
                "Class".PegiLabel(50).editEnum(ref cl).OnChanged(()=> ClassEnum = cl);

                if (Class.TryGetSubClass(out var subC))
                    subC.Inspect(ref subClass);

                pegi.nl();

                "Level".PegiLabel(60).write();
                Level.Nested_Inspect(fromNewLine: false).nl();

                "Gender".PegiLabel(60).editEnum(ref gender).nl();

                if (_manualName && "Use Fallback name".PegiLabel(toolTip: "Use default Name instead").ClickConfirm(confirmationTag: "ClearName").nl())
                    _manualName = false;
            }

            if ("Features".PegiLabel().isEntered().nl())
            {
                Feature[] all = (Feature[])Enum.GetValues(typeof(Feature));

                foreach (Feature f in all)
                {
                    if (this[f])
                        f.GetNameForInspector().PegiLabel().nl();
                }
            }

            "Feats".PegiLabel().enter_Inspect(Feats).nl();

            "Wallet".PegiLabel().enter_Inspect(Wallet).nl();

            if ("Other Proficiencies".PegiLabel().isEntered().nl())
            {
                using (_otherProfiiancies.StartContext())
                {
                    ArmorProficiencies.enter_Inspect().nl();
                    WeaponProficiencies.enter_Inspect().nl();
                    ToolProficiencies.enter_Inspect().nl();
                }
            }
        }

        public override void InspectInList(ref int edited, int ind)
        {
            "{0} {1}".F(RaceEnum, ClassEnum).PegiLabel(120).write();

            if (!_nameDecided && _manualName)
            {
                icon.Done.Click(() => _nameDecided = true, "Use This Name");
                icon.Clear.Click(() => _manualName = false, "Use Default Name");
            }
            base.InspectInList(ref edited, ind);
        }

        public string GetReadOnlyName() => "{0} a {1} {2} lvl {3} {4}".F(
            NameForInspector, 
            Race, 
            Class, 
            Level.Value,
            Class.TryGetSubClass(out var subC) ? " ({0})".F(subC.GetNameForInspector()) : ""
            );

        public override IEnumerator<object> SearchKeywordsEnumerator()
        {
            yield return base.SearchKeywordsEnumerator();

            yield return Class;
            yield return Race;
        }


        #endregion

        [Serializable]
        public class SmartId : DnD_SmartId<CharacterSheet>
        {
            protected override System.Collections.Generic.Dictionary<string, CharacterSheet> GetEnities() => Data.Characters;
        }
    }

    [Serializable]
    public class CharactersDictionary: SerializableDictionary<string, CharacterSheet> { }
}
