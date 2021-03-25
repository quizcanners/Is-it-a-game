using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public static class CharacterClassesExtensions
    {
        private static readonly Dictionary<Class, CharacterClass> allClasses = new()
        {
            {Class.Barbarian, new Barbarian() },
            {Class.Bard , new Bard() },
            {Class.Cleric , new Cleric() },
            {Class.Druid , new Druid() },
            {Class.Fighter , new Fighter() },
            {Class.Monk, new Monk() },
            {Class.Paladin , new Paladin() },
            {Class.Ranger , new Ranger() },
            {Class.Rogue , new Rogue() },
            {Class.Sorcerer , new Sorcerer() },
            {Class.Warlock , new Warlock() },
            {Class.Wizard , new Wizard() },
            {Class.Artificer , new Artificer() },
            {Class.BloodHunter , new BloodHunter() },
        };

        public static CharacterClass Get(this Class cl) => allClasses[cl];
    }

    public enum Class
    {
        Barbarian = 0,
        Bard = 1,
        Cleric = 2,
        Druid = 3,
        Fighter = 4,
        Monk = 5,
        Paladin = 6,
        Ranger = 7,
        Rogue = 8,
        Sorcerer = 9,
        Warlock = 10,
        Wizard = 11,
        Artificer = 12, 
        BloodHunter = 13,
    }

    public abstract class CharacterClass : ISearchable
    {
        protected virtual Dictionary<Feature, int> FeaturesForLevel { get; set; } = new Dictionary<Feature, int>();

        public bool HasFeature(Feature feature, int classLevel, int subClass)
        {
            if (FeaturesForLevel.TryGetValue(feature, out int levelNeeded) && levelNeeded <= classLevel)
                return true;

            if (TryGetSubClass(out SubClassCollectionBase sub) && sub.GetSubclass(subClass).HasFeature(feature, classLevel))
                return true;

            return false;
        }

        public virtual bool TryGetSubClass(out SubClassCollectionBase sc) 
        { 
            sc = null; 
            return false; 
        }

        public abstract Dice HitDie { get; }

        public virtual Proficiency this[AbilityScore stat] => Proficiency.None;

        public int GetStandardArrayBaseScoreFor(AbilityScore ability) =>
            AbilityScores.GetStandardArrayValue(GetStatPrioroty(ability));
       
        public virtual int GetStatPrioroty (AbilityScore priorityZeroIsHighest) 
        {
            switch (priorityZeroIsHighest) 
            {
                case AbilityScore.Dexterity:    return 0;
                case AbilityScore.Constitution: return 1;
                case AbilityScore.Strength:     return 2;
                case AbilityScore.Intelligence: return 3;
                case AbilityScore.Charisma:     return 4;
                case AbilityScore.Wisdom:       return 5;
                default: UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(priorityZeroIsHighest, context: "GetStatByPrioroty")); return 5;
            }
        }

        public virtual IEnumerator<object> SearchKeywordsEnumerator()
        {
            if (TryGetSubClass(out var sb))
                yield return sb;

            yield return GetType().ToString();
        }
    }

    public class Barbarian : CharacterClass
    {
        public override Dice HitDie => Dice.D12;

        protected override Dictionary<Feature, int> FeaturesForLevel { get; set; } = new Dictionary<Feature, int>()
            {
                { Feature.UnarmoredDefense, 1},
                { Feature.RecklessAttack, 2 },
                { Feature.DangerSense, 3 },
            };

        public override int GetStatPrioroty(AbilityScore priorityZeroIsHighest)
        {
            switch (priorityZeroIsHighest)
            {
                case AbilityScore.Dexterity: return 2;
                case AbilityScore.Constitution: return 1;
                case AbilityScore.Strength: return 0;
                case AbilityScore.Intelligence: return 4;
                case AbilityScore.Charisma: return 3;
                case AbilityScore.Wisdom: return 5;
                default: UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(priorityZeroIsHighest, context: "GetStatByPrioroty")); return 5;
            }
        }

        #region Subclasses
        public override bool TryGetSubClass(out SubClassCollectionBase sc)
        {
            sc = subclasses;
            return true;
        }

        readonly PrimalPathes subclasses = new();
        public enum PrimalPathEnum { PathOfTheBarserker, PathOfTheTotemWarrior  }

        public class PrimalPathes : SubClassCollectionGeneric<PrimalPathEnum>
        {
            protected override string SubClassName => "Primal Path";

            protected override SubClass Get(PrimalPathEnum subclass) 
            {
                switch (subclass) 
                {
                    case PrimalPathEnum.PathOfTheBarserker: return PathOfTheBarserker;
                    case PrimalPathEnum.PathOfTheTotemWarrior: return PathOfTheTotemWarrior;
                    default: Debug.LogError(QcLog.CaseNotImplemented(subclass, nameof(PrimalPathes))); return null;
                }
            }

            readonly SubClass PathOfTheBarserker = new()
            {
                FeaturesForLevel = new Dictionary<Feature, int>()
                {
                    { Feature.UnarmoredDefense, 1},
                    { Feature.RecklessAttack, 2 },
                    { Feature.DangerSense, 3 },
                }
            };
            readonly SubClass PathOfTheTotemWarrior = new()
            {
                FeaturesForLevel = new Dictionary<Feature, int>()
                {
                    { Feature.SpiritSeeker, 1},
                    { Feature.TotemSpirit, 2 },
                    { Feature.SpiritWalker, 3 },
                }
            };
        }

        #endregion
    }

    public class Bard : CharacterClass
    {
        public override Dice HitDie => Dice.D8;


        #region Subclasses
        public override bool TryGetSubClass(out SubClassCollectionBase sc)
        {
            sc = subclasses;
            return true;
        }

        readonly BardColleges subclasses = new();
        public enum BardCollegeEnum { CollegeOfLore, CollegeOfValor }

        public class BardColleges : SubClassCollectionGeneric<BardCollegeEnum>
        {
            protected override string SubClassName => "Bard College";

            protected override SubClass Get(BardCollegeEnum subclass)
            {
                switch (subclass)
                {
                    case BardCollegeEnum.CollegeOfLore: return CollegeOfLore;
                    case BardCollegeEnum.CollegeOfValor: return CollegeOfValor;
                    default: Debug.LogError(QcLog.CaseNotImplemented(subclass, nameof(BardColleges))); return null;
                }
            }

            readonly SubClass CollegeOfLore = new()
            {
                FeaturesForLevel = new Dictionary<Feature, int>()
                {
                    { Feature.UnarmoredDefense, 1},
                    { Feature.RecklessAttack, 2 },
                    { Feature.DangerSense, 3 },
                }
            };
            readonly SubClass CollegeOfValor = new()
            {
                FeaturesForLevel = new Dictionary<Feature, int>()
                {
                    { Feature.SpiritSeeker, 1},
                    { Feature.TotemSpirit, 2 },
                    { Feature.SpiritWalker, 3 },
                }
            };
        }

        #endregion

    }

    public class Cleric : CharacterClass
    {
        public override Dice HitDie => Dice.D8;
    }

    public class Druid : CharacterClass
    {
        public override Dice HitDie => Dice.D8;
    }

    public class Fighter : CharacterClass
    {
        public override Dice HitDie => Dice.D10;
    }

    public class Monk : CharacterClass
    {
        public override Dice HitDie => Dice.D8;
    }

    public class Paladin : CharacterClass
    {
        public override Dice HitDie => Dice.D10;
    }

    public class Ranger : CharacterClass
    {
        public override Dice HitDie => Dice.D10;
    }

    public class Rogue : CharacterClass
    {
        public override Dice HitDie => Dice.D8;
    }

    public class Sorcerer : CharacterClass
    {
        public override Dice HitDie => Dice.D6;
    }

    public class Warlock : CharacterClass
    {
        public override Dice HitDie => Dice.D8;
    }

    public class Wizard : CharacterClass
    {
        public override Dice HitDie => Dice.D6;
    }

    public class Artificer : CharacterClass
    {
        public override Dice HitDie => Dice.D8;
    }

    public class BloodHunter : CharacterClass
    {
        public override Dice HitDie => Dice.D10;
    }




    public abstract class SubClassCollectionBase
    {
        public abstract SubClass GetSubclass(int subclass);

        public abstract void Inspect(ref int selectedSubclass);

        public class SubClass
        {
            public Dictionary<Feature, int> FeaturesForLevel; // { get; set; } = new Dictionary<Feature, int>();
            public bool HasFeature(Feature feature, int classLevel) => FeaturesForLevel.TryGetValue(feature, out int levelNeeded) && levelNeeded <= classLevel;
        }
    }

    public abstract class SubClassCollectionGeneric<T> : SubClassCollectionBase, IGotReadOnlyName
    {
        protected abstract string SubClassName { get; }

        

        public override void Inspect(ref int selectedSubclass)
        {
            SubClassName.PegiLabel(90).editEnum<T>(ref selectedSubclass);
        }

        public override SubClass GetSubclass(int subclass)
        {
            var sub = Get(QcSharp.ToEnum<T>(subclass));
           
            if (sub == null)
                sub = Get(QcSharp.ToEnum<T>(0));

            return sub;
        }
        protected abstract SubClass Get(T subclass);

        public string GetReadOnlyName() => SubClassName;
    }



}