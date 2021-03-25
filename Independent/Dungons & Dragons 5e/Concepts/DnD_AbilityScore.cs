using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    [Serializable] public class AbilityScores : SerializableDictionary_ForEnum<AbilityScore, CoreStat> 
    {
        public static int GetStandardArrayValue (int priorityIndex) 
        {
            switch (priorityIndex) 
            {
                case 0: return 15;
                case 1: return 14;
                case 2: return 13;
                case 3: return 12;
                case 4: return 10;
                case 5: return 8;

                default:
                    Debug.LogError(QcLog.CaseNotImplemented(priorityIndex, nameof(GetStandardArrayValue))); 
                    return 10;
            }
        }


        protected override void InspectElementInList(AbilityScore key, int index)
        {

            var value = this.TryGet(key);

            string name = "{0} ({1})".F(key.GetShortName(), Creature.inspectedCreature[key].ToSignedNumber());

            if (value == null)
            {
                int current = Creature.inspectedCreature.GetDefaultValue(key);
                if ("{0} (Default)".F(name).PegiLabel(120).editDelayed(ref current) && current > 0)
                    this[key] = new CoreStat(current);
            }
            else
            {
                if (icon.Clear.Click())
                    Remove(key);
                else
                    name.PegiLabel(110).edit(ref value.Value, valueWidth: 35);
            }

            icon.Dice.Click(()=> 
                pegi.GameView.ShowNotification(Creature.inspectedCreature.RollSavingThrow(key, influence: RollInfluence.None).ToString()));

        }
    }

    public enum AbilityScore
    {
        Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma
    }

    public static class DnDStatExtensions 
    {
        public static string GetShortName(this AbilityScore stat) 
        {
            return stat switch
            {
                AbilityScore.Strength => "Str",
                AbilityScore.Dexterity => "Dex",
                AbilityScore.Constitution => "Con",
                AbilityScore.Intelligence => "Int",
                AbilityScore.Wisdom => "Wis",
                AbilityScore.Charisma => "Cha",
                _ => "Err",
            };
        }
    }

    [Serializable]
    public class CoreStat 
    {
        public int Value;


        public CoreStat() { }

        public CoreStat(RollResult value) { Value = value.Value; }

        public CoreStat(int value) { Value = value; }
    }

}