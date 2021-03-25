using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;

namespace Dungeons_and_Dragons
{
    public enum Proficiency
    {
        None = 0, Normal = 1, Expertiese = 2
    }

    [Serializable] public class SkillSet : ProficiencyDictionary<Skill> { }
    [Serializable] public class SavingThrowProficiencies : ProficiencyDictionary<AbilityScore> { }
    [Serializable] public class ArmorProficiencies : ProficiencyDictionary<ArmorType> { }
    [Serializable] public class WeaponProficiencies : ProficiencyDictionary<WeaponTypw> { }
    [Serializable] public class ToolProficiencies : ProficiencyDictionary<Tools> { }

    [Serializable]
    public abstract class ProficiencyDictionary<T> : SerializableDictionary<T, Proficiency> , IPEGI, ISearchable
    {
        new public Proficiency this[T skill]
        {
            get
            {
                if (TryGetValue(skill, out Proficiency prof))
                    return prof;

                return Proficiency.None;
            }
            set
            {
                if (value == Proficiency.None)
                    Remove(skill);
                else
                    base[skill] = value;
            }
        }

        public void Inspect(T skill)
        {
            var changed = pegi.ChangeTrackStart();
            var val = this[skill];

            if (val == Proficiency.None)
                val.GetIcon().Click(()=> val = Proficiency.Normal);

            if (val == Proficiency.Normal)
                val.GetIcon().Click(()=> val = Proficiency.None);

            if (val == Proficiency.Expertiese)
                val.GetIcon().draw();

            pegi.editEnum(ref val, width: 60);

            if (changed)
                this[skill] = val;
        }

        public override void Inspect()
        {
            var type = typeof(T);

            type.GetNameForInspector().PegiLabel(style: pegi.Styles.ListLabel).nl();

            var skills = (T[])Enum.GetValues(typeof(T));

            foreach (var skill in skills)
            {
                Inspect(skill);

                "{0}".F(QcSharp.AddSpacesInsteadOfCapitals(skill.ToString())).PegiLabel().write();

                pegi.nl();
            }

            pegi.line();
        }

        public IEnumerator<object> SearchKeywordsEnumerator()
        {
            foreach (var el in this)
                yield return el.Key.ToString();
        }
    }

    public static class DnD_ProficienciesExtensions 
    {
        public static Proficiency And<T>(this ProficiencyDictionary<T> skillSet, ProficiencyDictionary<T> skillSet2, T type)
        {
            if (skillSet2 != null)
            {
                return skillSet[type].And(skillSet2[type]);
            }

            return skillSet2[type];
        }
        
        public static Proficiency And(this Proficiency proficiency, Proficiency other) => proficiency > other ? proficiency : other;

        public static icon GetIcon (this Proficiency proficiency) 
        {
            return proficiency switch
            {
                Proficiency.Expertiese => icon.Book,
                Proficiency.Normal => icon.Active,
                Proficiency.None => icon.InActive,
                _ => icon.Question,
            };
        }
    }

    public enum ArmorType { LightArmor = 0, MediumArmor = 1, HeavyArmor = 2 }

    public enum WeaponTypw { Crossbow, Hand, Longsword, Rapier, Shortsword, SimpleWeapos }

    public enum Tools { DisguiseKit, PlayingCardSet, ThievesTools}

}