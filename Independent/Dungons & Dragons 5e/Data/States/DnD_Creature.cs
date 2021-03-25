using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public abstract class Creature : IPEGI, IGotName, IPEGI_ListInspect, IConceptValueProvider, ISearchable
    {
        protected const int STATS_COUNT = 6;

        [SerializeField] public RanDndSeed Seed = new();
        [SerializeField] protected string _name;

        [SerializeField] public Allignment Allignment = new();
        [SerializeField] protected AbilityScores Stats = new();
        [SerializeField] public List<Language> LanguagesKnown = new() { Language.Common };
        [SerializeField] protected SkillSet skillSet = new();
        [SerializeField] protected SavingThrowProficiencies savingThrowProficiencies = new();
        [SerializeField] private SensesDictionary senses = new();

        public abstract GridDistance this[SpeedType type] { get; }
        public bool this[Language lang] 
        {
            get => LanguagesKnown.Contains(lang);
            set 
            {
                var contains = LanguagesKnown.Contains(lang);
                if (value && !contains) 
                {
                    LanguagesKnown.Add(lang);
                } else if (!value && contains) 
                {
                    LanguagesKnown.Remove(lang);
                }    
            }
        }
        public abstract Size Size { get; }
        public GridDistance this [Sense sense]  => senses.TryGet(sense);
        protected int this[Proficiency prof]
        {
            get
            {
                switch (prof)
                {
                    case Proficiency.None: return 0;
                    case Proficiency.Normal: return ProficiencyBonus;
                    case Proficiency.Expertiese: return ProficiencyBonus * 2;
                    default:
                        Debug.LogError(QcLog.CaseNotImplemented(prof,  "Creature")); return 0;
                }
            }
        }
        protected abstract int ProficiencyBonus { get; }
        public abstract int ArmorClass { get; }
       
        public abstract int MaxHitPoints { get; }

        public virtual bool this[Trait trait] => false;

        internal int this[AbilityScore stat] => Mathf.FloorToInt((GetTotalScore(stat) - 10) / 2f);

        #region Ability Scores
        protected int GetAbilityBaseScore(AbilityScore stat)
        {
            if (Stats.TryGetValue(stat, out var st))
                return st.Value;
            else
                return GetDefaultValue(stat);
        }
        internal abstract int GetDefaultValue(AbilityScore stat);
        internal int GetTotalScore(AbilityScore stat) => GetAbilityBaseScore(stat) + GetAbilityScoreRacialBonus(stat);
        protected abstract int GetAbilityScoreRacialBonus(AbilityScore stat);
        #endregion

        #region Rolls
        //public abstract int RollMaxHp();
        public RollResult Roll(Skill skill, RollInfluence influence = RollInfluence.None) => Roll(skill.GetDefaultRelevantAbility(), skill, influence);

        public virtual RollResult Roll(AbilityScore ability, RollInfluence influence = RollInfluence.None) => Dice.D20.Roll(influence: influence) + this[ability];
        public RollResult Roll(AbilityScore ability, Skill skill, RollInfluence influence = RollInfluence.None) => Roll(ability, influence) + this[GetProficiency(skill)];

        public virtual RollResult RollSavingThrow(AbilityScore stat, RollInfluence influence = RollInfluence.None) => Dice.D20.Roll(influence: influence) + SavingThrowBonus(stat);
        #endregion

        public int CalculateDamage(int damage, DamageType damageType) => GetDamageResistance(damageType).ModifyDamage(damage);

        public virtual DamageResistance GetDamageResistance(DamageType damageType) => DamageResistance.None;
        protected virtual int SavingThrowBonus(AbilityScore stat) => this[stat] + this[SavingThrowProficiency(stat)];

        protected virtual Proficiency SavingThrowProficiency(AbilityScore stat) => savingThrowProficiencies[stat];
        protected virtual Proficiency GetProficiency(Skill stat) => skillSet[stat];
      
        protected void RerollAbilityScores() 
        {
            for (int i = 0; i < STATS_COUNT; i++)
                Stats[(AbilityScore)i] = new CoreStat(Dice.D6.Roll(4, Drop.Lowest));
        }

        public virtual bool TryGetAttack(Weapon.SmartId weapon, out Attack attack) 
        {
            if (weapon == null) 
            {
                attack = null;
                return false;
            }

            if (!weapon.TryGetEntity(out Weapon prot)) 
            {
                attack = null;
                return false;
            }

            var ability = (prot[Weapon.Property.Finesse] ? Math.Max(this[AbilityScore.Strength], this[AbilityScore.Dexterity]) : this[AbilityScore.Strength]);

            attack= new Attack(
                name: prot.NameForInspector,
                  isRange: prot.IsRanged,
                attackBonus: ProficiencyBonus + ability,
                damageDice: new List<Dice>() { prot.Damage },
                damageBonus: ability,
                damageType: prot.DamageType
            );

            return true;
        }

        public abstract List<Attack> GetAttacks();

        public virtual bool TryGetConcept<T>(out T value) where T : IComparable
        {
            if (typeof(T) == typeof(Size)) 
            {
                value = (T)((object)Size);
                return true;
            }

            value = default;
            return false;
        }

        #region Inspector

        protected virtual void Inspect_HeaderBlock()
        {
            NameForInspector.PegiLabel(style: pegi.Styles.HeaderText).nl();
        }

        protected virtual void Inspect_AcHpSpeedBlock()
        {
            "Armor Class {0}".F(ArmorClass).PegiLabel().nl();
            "Hit Points {0}".F(MaxHitPoints).PegiLabel().nl();
            "Speed {0}".F(this[SpeedType.Walking]).PegiLabel().nl();

        }

        protected virtual void Inspect_AbiitiesBlock()
        {
            var ablNames = new System.Text.StringBuilder().Append("   ");
           // var ablValues = new System.Text.StringBuilder();
            for (int i=0; i<6; i++) 
            {
                var ab = (AbilityScore)i;
                ablNames.Append(ab.GetShortName()).Append("      ");
                var modifier = this[ab];
                string abilityScoreText = "{0}({1})".F(GetTotalScore(ab), modifier >= 0 ? "+{0}".F(modifier) : modifier.ToString());

                "{0}{1}{2}".F(ab.GetShortName(), pegi.EnvironmentNl, abilityScoreText).PegiLabel(width: 60, style: pegi.Styles.ListLabel).write();

                //ablValues.Append(abilityScoreText).Append(") ");
            }
            //ablNames.ToString().nl();
            //ablValues.ToString().nl();

        }

        protected virtual void Inspect_ProficienciesBlock()
        {
            "Proficiency Bonus: + {0}".F(ProficiencyBonus).PegiLabel().nl();
        }

        protected virtual void Inspect_Actions(List<Attack> attacks)
        {
            if (attacks.Count == 0)
                return;

            "Actions".PegiLabel(style: pegi.Styles.ListLabel).nl();
            pegi.line(Color.red);

            foreach (var a in attacks) 
            {
                a.GetNameForInspector().PegiLabel().writeBig();
            }
        }

        public virtual void Inspect_StatBlock(List<Attack> attacks) 
        {
            Inspect_HeaderBlock();

            pegi.line(Color.red);

            Inspect_AcHpSpeedBlock();

            pegi.line(Color.red);

            Inspect_AbiitiesBlock();

            pegi.line(Color.red);

            Inspect_ProficienciesBlock();

            Inspect_Actions(attacks);
        }

        internal static Creature inspectedCreature;

        protected pegi.EnterExitContext enterExitContext = new();
        //protected int inspectedStuff = -1;
        public virtual string NameForInspector { get => _name; set => _name = value; }

        protected virtual void Inspect_Contextual() {}



        public void Inspect()
        {
            using (enterExitContext.StartContext())
            {
                inspectedCreature = this;

                if (enterExitContext.IsAnyEntered == false)
                {
                    var tmpName = NameForInspector;
                    if ("Name [Key]".PegiLabel(90).edit(ref tmpName))
                        NameForInspector = tmpName;

                    var tmp = this;
                    pegi.CopyPaste.InspectOptionsFor(ref tmp);
                    if (icon.Dice.ClickConfirm(confirmationTag: "Change Random seed? Will affec all Fallback values"))
                        Seed.Randomize();
                    pegi.nl();
                }

                if ("Block".PegiLabel().isEntered().nl())
                    Inspect_StatBlock(GetAttacks());

                if ("Ability Scores [{0}]".F(Stats.IsNullOrEmpty() ? "Fallback" : "Manual").PegiLabel().isEntered().nl_ifEntered())
                {
                    if (Stats.Count > 0 && icon.Clear.ClickConfirm("clStts", toolTip: "Will Clear All Stat Rolls"))
                        Stats.Clear();

                    if ("Reroll".PegiLabel(toolTip: "This will rerolls all your Ability Scores Base Values.").ClickConfirm(confirmationTag: "ReRollSt").nl())
                        RerollAbilityScores();

                    Stats.Nested_Inspect().nl();
                }

                if (enterExitContext.IsCurrentEntered && Stats.IsNullOrEmpty())
                    icon.Dice.Click(RerollAbilityScores);

                pegi.nl();

                if ("Skills".PegiLabel().isEntered().nl())
                {
                    var skills = (Skill[])Enum.GetValues(typeof(Skill));

                    for (int i = 0; i < skills.Length; i++)
                    {
                        Skill skill = skills[i];

                        skillSet.Inspect(skill);

                        "{0} {1} ({2})".F((this[GetProficiency(skill)] + this[skill.GetDefaultRelevantAbility()]).ToSignedNumber(), skill.GetNameForInspector(), skill.GetDefaultRelevantAbility().GetShortName()).PegiLabel(120).write();

                        icon.Dice.Click(() => pegi.GameView.ShowNotification(Roll(skill).ToString()));

                        pegi.nl();
                    }
                }

                if ("Saving Throws".PegiLabel().isEntered().nl())
                {
                    for (int i = 0; i < STATS_COUNT; i++)
                    {
                        var enm = ((AbilityScore)i);

                        savingThrowProficiencies.Inspect(enm);

                        "{0} ({1})".F(enm.ToString(), SavingThrowBonus(enm).ToSignedNumber()).PegiLabel().write();

                        icon.Dice.Click(() =>
                            pegi.GameView.ShowNotification(RollSavingThrow(enm).ToString()));

                        pegi.nl();
                    }
                }

                "Senses".PegiLabel().isEntered().nl().If_Entered(()=> senses.Nested_Inspect());
                
                Allignment.ToString().PegiLabel().enter_Inspect(Allignment).nl();

                Inspect_Contextual();
            }
        }

        public virtual void InspectInList(ref int edited, int ind)
        {
            var name = NameForInspector;
            if (pegi.edit(ref name))
                NameForInspector = name;

            if (icon.Enter.Click())
                edited = ind;
        }

        public virtual IEnumerator<object> SearchKeywordsEnumerator()
        {
            foreach (var l in LanguagesKnown)
                yield return l.ToString();

            yield return skillSet;
        }

        #endregion

    }
}