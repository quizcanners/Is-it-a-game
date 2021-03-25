
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [System.Serializable]
    public class SkillCheck : IPEGI, IGotReadOnlyName
    {
        [SerializeField] AbilityFallback _abilityScore = new();
        public Skill SkillToCheck = Skill.Perception;
        public int DC = 15;


        [Serializable]
        private class AbilityFallback : Fallback.FallbackValueGeneric<AbilityScore> { }

        public AbilityScore AbilityScore
        {
            get => _abilityScore[defaultValueGetter: ()=> SkillToCheck.GetDefaultRelevantAbility()]; 
            set => _abilityScore.ManualValue = value;
        }

        public bool Check(Creature creature, RollInfluence influence) => creature.Roll(AbilityScore, SkillToCheck, influence) >= DC;

        public void Inspect()
        {
            "DC".PegiLabel(40).edit( ref DC, 50);

            if (_abilityScore.IsSet)
            {
                if (icon.Clear.Click("Use Default Ability"))
                    _abilityScore.Clear();

                var score = AbilityScore;
                if (pegi.editEnum(ref score, width: 70))
                    AbilityScore = score;
            } else 
            {
                if (AbilityScore.GetNameForInspector().PegiLabel("Change Ability to test").Click())
                    AbilityScore = SkillToCheck.GetDefaultRelevantAbility();
            }

            pegi.editEnum(ref SkillToCheck, width: 70);
        }

        public string GetReadOnlyName() => "DC {0} {1} ({2})".F(DC, AbilityScore, SkillToCheck);
    }
}