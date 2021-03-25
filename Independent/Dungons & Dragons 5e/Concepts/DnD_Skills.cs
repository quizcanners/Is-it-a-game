using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons {

    public static class SkillsExtensions 
    {
        public static AbilityScore GetDefaultRelevantAbility (this Skill skill) 
        {
            switch (skill) 
            {
                case Skill.Acrobatics:        return AbilityScore.Dexterity;
                case Skill.Animal_Handling:   return AbilityScore.Wisdom;
                case Skill.Arcana:            return AbilityScore.Intelligence;
                case Skill.Athletics:         return AbilityScore.Strength;
                case Skill.Deception:         return AbilityScore.Charisma;
                case Skill.History:           return AbilityScore.Intelligence;
                case Skill.Insight:           return AbilityScore.Wisdom;
                case Skill.Intimidation:      return AbilityScore.Charisma;
                case Skill.Investigation:     return AbilityScore.Intelligence;
                case Skill.Medicine:          return AbilityScore.Wisdom;
                case Skill.Nature:            return AbilityScore.Intelligence;
                case Skill.Perception:        return AbilityScore.Wisdom;
                case Skill.Performance:       return AbilityScore.Charisma;
                case Skill.Persuasion:        return AbilityScore.Charisma;
                case Skill.Religion:          return AbilityScore.Intelligence;
                case Skill.Sleight_of_Hand:   return AbilityScore.Dexterity;
                case Skill.Stealth:           return AbilityScore.Dexterity;
                case Skill.Survival:          return AbilityScore.Wisdom;
                default: Debug.LogError("{0} skill not implemented".F(skill)); return AbilityScore.Wisdom;
            }   
        }
    }



    public enum Skill 
    {
        Acrobatics = 1,
        Animal_Handling = 2,
        Arcana = 3,
        Athletics = 4,
        Deception = 5,
        History = 6,
        Insight = 7,
        Intimidation = 8,
        Investigation = 9,
        Medicine = 10,
        Nature = 11,
        Perception = 12,
        Performance = 13,
        Persuasion = 14,
        Religion = 15,
        Sleight_of_Hand = 16,
        Stealth = 17,
        Survival = 18,
    }

}