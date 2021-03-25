using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeons_and_Dragons.Calculators
{
    [Serializable]
    public class CombatTracker : IPEGI
    {
        public List<MonsterState> enemies = new();
        public List<CharacterState> characters = new();

        [SerializeField] private bool _combatStarted;

        private int _inspectedEnemy = -1;
        private int _inspectedCharacter = -1;
        private int _inspectedInInitiative = -1;

        public int ScaledExperience
        {
            get 
            {
                int total = 0;
                int count = 0;
                foreach(var e in enemies) 
                {
                    var m = e.Creature;
                    if (m != null)
                    {
                        total += m.ChallengeRating.GetExperience();
                        count++;
                    }
                }

                return (int)(total * EncounterCalculator.MultiplierByMonsterCount(count, characters.Count));
            } 
        }

        void RollInitiative() 
        {
            _combatStarted = true;

            foreach (var e in enemies)
                e.RollInitiative();

            foreach (var c in characters)
                c.RollInitiative();
        }

        void RestartCombat() 
        {
            _combatStarted = false;

            foreach (var e in enemies)
                e.LongRest();

            foreach (var c in characters)
                c.LongRest();
        }

        protected void DealDamage(CreatureStateBase attacker, CreatureStateBase target, RollInfluence influence) 
        {
            var atcks = attacker.GetAttacksList();
            if (atcks.Count == 0)
                return;

            Attack strongest = atcks[0];

            for (int i = 0; i < atcks.Count; i++)
                if (atcks[i].AvarageDamage > strongest.AvarageDamage)
                    strongest = atcks[i];

            target.TryTakeHit(strongest, influence);
        }

        public void Inspect()
        {
            if (_combatStarted)
            {
                Dictionary<int, List<CreatureStateBase>> _byInitiative = new();

                foreach (var p in characters)
                    if (p!= null)
                        _byInitiative.GetOrCreate(p.Initiative).Add(p);

                foreach (var e in enemies)
                    if (e != null)
                        _byInitiative.GetOrCreate(e.Initiative).Add(e);

                List<int> initiatives = _byInitiative.Keys.ToList();
                initiatives.Sort((a,b) => b-a);

                int currentIndex = -1;
                foreach(var i in initiatives) 
                {
                    var els = _byInitiative[i];
                    foreach(var el in els) 
                    {
                        if (_inspectedInInitiative == -1)
                        {
                            el.Initiative.ToString().PegiLabel(20).write();
                        }
                        el.Inspect_AsInListNested(ref _inspectedInInitiative, ++currentIndex).nl();
                        pegi.nl();
                    }
                }


                icon.Refresh.Click(RestartCombat);
                if ("Enemy Hit".PegiLabel().Click())
                    DealDamage(enemies.GetRandom(), characters.GetRandom(), influence: RollInfluence.None);
                if ("Party Hit".PegiLabel().Click())
                    DealDamage(characters.GetRandom(), enemies.GetRandom(), influence: RollInfluence.None);
            }
            else
            {
                if (_inspectedEnemy != -1)
                {
                    _inspectedCharacter = -1;
                }

                if (_inspectedCharacter == -1)
                    "Enemies".PegiLabel().edit_List(enemies, ref _inspectedEnemy).nl();

                if (_inspectedCharacter == -1 && _inspectedEnemy == -1)
                {
                    var budgets = EncounterCalculator.CalculateExpBudgets(characters, c => c.Creature?.Level);
                    int scaledExp = ScaledExperience;
                    EncounterDifficulty difficulty = EncounterCalculator.GetDifficulty(budgets, scaledExp);
                    EncounterCalculator.InspectDrawDifficultyProgressBar(difficulty, totalMonsterExp: scaledExp, budgets);

                    "Roll Initiative!".PegiLabel().Click(RollInitiative);
                    pegi.nl();
                }

                if (_inspectedEnemy == -1)
                    "Party".PegiLabel().edit_List(characters, ref _inspectedCharacter).nl();
            }
        }
    }
}