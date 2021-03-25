using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons.Calculators
{

    [Serializable]
    public class EncounterCalculator : IPEGI
    {
        [SerializeField] private List<FriendlyCharacter> PlayerCharacters = new List<FriendlyCharacter>();
        [SerializeField] private List<EnemyMonster> Monsters = new List<EnemyMonster>();

        public static float MultiplierByMonsterCount (int monsterCount, int partySize) 
        {
            if (monsterCount == 1) return MultiplierByIndex(0);
            if (monsterCount == 2) return MultiplierByIndex(1);
            if (monsterCount <= 6) return MultiplierByIndex(2);
            if (monsterCount <= 10) return MultiplierByIndex(3);
            if (monsterCount <= 14) return MultiplierByIndex(4);

            return MultiplierByIndex(5);

            float MultiplierByIndex(int index)
            {
                var mltp = GetPartySize(partySize);
                switch (mltp) 
                {
                    case PartySize.Small: index += 1; break;
                    case PartySize.Large: index -= 1; break;
                }

                index = Mathf.Clamp(index, 0, 6);

                return index switch
                {
                    0 => 1,
                    1 => 1.5f,
                    2 => 2f,
                    3 => 2.5f,
                    4 => 3f,
                    5 => 4f,
                    _ => 5,
                };
            }
        }

        public static PartySize GetPartySize(int count) 
        {

            if (count < 3)
                return PartySize.Small;
            if (count < 6)
                return PartySize.Avarage;
            return PartySize.Large;
        }

        public enum PartySize { Small, Avarage, Large }

        public static int[] CalculateExpBudgets<T>(List<T> characters, Func<T,CharacterLevel> levelGetter) 
        {
            int[] expBudgets = new int[4];
            for (int difficulty = 0; difficulty < 4; difficulty++)
            {
                var diff = (EncounterDifficulty)difficulty;
                foreach (var p in characters)
                    if (p != null)
                    {
                        CharacterLevel result = levelGetter(p);
                        if (result != null)
                        {
                            expBudgets[difficulty] += result.GetExperienceThresholdFor(diff);
                        }
                    }
            }

            return expBudgets;
        }

        public static EncounterDifficulty GetDifficulty(int[] budgets, int totalMonsterExp) 
        {
            var currentDifficulty = EncounterDifficulty.Easy;
            while (currentDifficulty < EncounterDifficulty.Deadly && budgets[(int)currentDifficulty] < totalMonsterExp)
                currentDifficulty++;

            return currentDifficulty;
        }


        #region Inspector

        private int _inspectedCharacter = -1;
        private int _inspectedMonster = -1;
        public void Inspect()
        {
            if (_inspectedCharacter != -1)
                _inspectedMonster = -1;

            if (_inspectedMonster == -1)
                "Party".PegiLabel().edit_List(PlayerCharacters, ref _inspectedCharacter);

            if (_inspectedMonster == -1 && _inspectedCharacter == -1)
            {
                // Count Monsters
                int monsterCount = 0;
                int totalMonstersExp = 0;
                foreach (var m in Monsters)
                {
                    totalMonstersExp += m.TotalExperience;
                    monsterCount += m.Count;
                }

                float mltp = MultiplierByMonsterCount(monsterCount, PlayerCharacters.Count);

                totalMonstersExp = (int)(totalMonstersExp * mltp);

                // Count Player Budgets
                int[] budgets = CalculateExpBudgets(PlayerCharacters, ch => ch.Level);
                EncounterDifficulty currentDifficulty = GetDifficulty(budgets, totalMonstersExp);

                InspectDrawDifficultyProgressBar(currentDifficulty, totalMonstersExp, budgets);
            }

            if (_inspectedCharacter == -1)
                "Monsters".PegiLabel().edit_List(Monsters, ref _inspectedMonster);
            
        }

        #endregion

        public static void InspectDrawDifficultyProgressBar(EncounterDifficulty difficulty, int totalMonsterExp, int[] budgets) 
        {
            int budget = budgets[(int)difficulty];

            Color col;

            switch (difficulty)
            {
                case EncounterDifficulty.Easy: col = Color.green; break;
                case EncounterDifficulty.Medium: col = Color.blue; break;
                case EncounterDifficulty.Hard: col = Color.yellow; break;
                case EncounterDifficulty.Deadly: col = Color.red; break;
                default: col = Color.gray; break;
            }

            using (pegi.SetBgColorDisposable(col))
            {
                float portion = budget > 0 ? ((float)totalMonsterExp / budget) : 1;
                "{0}  (Exp : {1}/{2})".F(difficulty, totalMonsterExp, budget).PegiLabel().drawProgressBar(portion);
                pegi.nl();
            }
        }

        [Serializable]
        protected class FriendlyCharacter : IPEGI_ListInspect, IGotName, IPEGI
        {
            [SerializeField] private string _fallbackName = "Bob";
            [SerializeField] private CharacterLevel _fallbackLevel = new CharacterLevel();

            [SerializeField] private CharacterSheet.SmartId _chracterId = new CharacterSheet.SmartId();
            [SerializeField] private bool _useCharacter;

            public int this[EncounterDifficulty difficulty] => Level.GetExperienceThresholdFor(difficulty: difficulty);

            public string NameForInspector 
            {
                get => _chracterId.TryGetValue(sheet => sheet.NameForInspector, () => _fallbackName);
                set => _chracterId.TrySetValue(sheet => sheet.NameForInspector = value, ()=> _fallbackName = value);
            }

            public CharacterLevel Level
            {
                get => _chracterId.TryGetValue(sheet => sheet.Level, () => _fallbackLevel);
                set => _chracterId.TrySetValue(sheet => sheet.Level = value, () => _fallbackLevel = value);
            }

      
            #region Inspector
            public void InspectInList(ref int edited, int index)
            {
                if (!_useCharacter)
                    icon.Search.Click(() => _useCharacter = true);
                else
                    icon.Clear.Click(() => _useCharacter = false);

                if (_useCharacter)
                {
                    _chracterId.InspectInList(ref edited, index);
                }
                else
                {
                    this.inspect_Name();
                    Level.Inspect();
                }
            }

            public void Inspect()
            {
                _chracterId.Inspect();
            }
            #endregion
        }

        [Serializable]
        protected class EnemyMonster : IPEGI_ListInspect, IGotName, IPEGI
        {
            [SerializeField] private string _fallbackName = "Bandit";
            [SerializeField] private ChallangeRating fallbackCR = ChallangeRating.CR_1;

            [SerializeField] private Monster.SmartId _monsterId = new Monster.SmartId();
            [SerializeField] private bool _useMonster;

            public int Count = 1;

            public ChallangeRating CR 
            {
                get => _monsterId.TryGetValue(sheet => sheet.ChallengeRating, () => fallbackCR);
                set => _monsterId.TrySetValue(sheet => sheet.ChallengeRating = value, () => fallbackCR = value);
            }

            public int TotalExperience => CR.GetExperience() * Count;

            public string NameForInspector
            {
                get => _monsterId.TryGetValue(sheet => sheet.NameForInspector, () => _fallbackName);
                set => _monsterId.TrySetValue(sheet => sheet.NameForInspector = value, () => _fallbackName = value);
            }

            #region Inspector

            public void InspectInList(ref int edited, int index)
            {
                "Cnt:".PegiLabel(35).edit_WithButtons(ref Count, 30);

                if (!_useMonster)
                    icon.Search.Click(() => _useMonster = true);
                else
                    icon.Clear.Click(() => _useMonster = false);

                if (_useMonster)
                    _monsterId.InspectInList(ref edited, index);
                else
                {
                    "CR".PegiLabel(35).editEnum(ref fallbackCR, x => x.GetReadableString(), width: 30);
                    this.inspect_Name();
                }
            }

            public void Inspect()
            {
                _monsterId.Nested_Inspect();
            }

            #endregion
        }
    }
}
