using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class CharacterLevel : IPEGI
    {
        [SerializeField] private int _value = 1;

        public int Value 
        {
            get => _value;
            set => _value = Mathf.Clamp(value, 1, 20);
        }

        public static implicit operator int(CharacterLevel l) => l.Value;

        public int ExperienceForNextLevel => GetExperienceForLevel(Value + 1);

        public int GetExperienceForLevelUp() => GetExperienceForLevel(Value);

        public int ProficiencyBonus
        {
            get
            {
                if (Value < 5) return 2;
                if (Value < 9) return 3;
                if (Value < 13) return 4;
                if (Value < 17) return 5;
                return 6;
            }
        }

        public int TierOfPlay
        {
            get
            {
                if (Value < 5) return 1;
                if (Value < 11) return 2;
                if (Value < 17) return 3;
                return 4;
            }
        }

        public int GetExperienceForLevel(int Level)
        {
            switch (Level)
            {
                case 1: return 0;
                case 2: return 300;
                case 3: return 900;
                case 4: return 2700;
                case 5: return 6500;
                case 6: return 14000;
                case 7: return 23000;
                case 8: return 34000;
                case 9: return 48000;
                case 10: return 64000;
                case 11: return 85000;
                case 12: return 100000;
                case 13: return 120000;
                case 14: return 140000;
                case 15: return 165000;
                case 16: return 195000;
                case 17: return 225000;
                case 18: return 265000;
                case 19: return 305000;
                case 20: return 355000;
                default:
                    Debug.LogError("No Exp Thold for Level {0}".F(Level));
                    return int.MaxValue;
            }
        }

        public int GetExperienceThresholdFor(EncounterDifficulty difficulty) 
        {
            int mltp = (int)difficulty + 1;

            switch (Value) 
            {
                case 1: //1st	25	50	75	100
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 25,
                        EncounterDifficulty.Medium => 50,
                        EncounterDifficulty.Hard => 75,
                        _ => 100,
                    };
                case 2: //   2nd	50	100	150	200
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 50,
                        EncounterDifficulty.Medium => 100,
                        EncounterDifficulty.Hard => 150,
                        _ => 200,
                    };
                case 3: // 3rd	75	150	225	400
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 75,
                        EncounterDifficulty.Medium => 150,
                        EncounterDifficulty.Hard => 225,
                        _ => 400,
                    };
                case 4: //4th	125	250	375	500
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 125,
                        EncounterDifficulty.Medium => 250,
                        EncounterDifficulty.Hard => 375,
                        _ => 500,
                    };
                case 5:  //5th	250	500	750	1100
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 250,
                        EncounterDifficulty.Medium => 500,
                        EncounterDifficulty.Hard => 750,
                        _ => 1100,
                    };
                case 6: //6th	300	600	900	1400
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 300,
                        EncounterDifficulty.Medium => 600,
                        EncounterDifficulty.Hard => 900,
                        _ => 1400,
                    };
                case 7: //7th	350	750	1100	1700
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 350,
                        EncounterDifficulty.Medium => 750,
                        EncounterDifficulty.Hard => 1100,
                        _ => 1700,
                    };
                case 8:  //8th	450	900	1400     2100
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 450,
                        EncounterDifficulty.Medium => 900,
                        EncounterDifficulty.Hard => 1400,
                        _ => 2100,
                    };
                case 9: // 9th    550 1100   1600   2400
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 550,
                        EncounterDifficulty.Medium => 1100,
                        EncounterDifficulty.Hard => 1600,
                        _ => 2400,
                    };
                case 10: //10th	600	1200	1900	2800
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 600,
                        EncounterDifficulty.Medium => 1200,
                        EncounterDifficulty.Hard => 1900,
                        _ => 2800,
                    };
                case 11: //11th	800	1600	2400	3600
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 800,
                        EncounterDifficulty.Medium => 1600,
                        EncounterDifficulty.Hard => 2400,
                        _ => 3600,
                    };
                case 12: //12th	1000	2000	3000	4500
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 1000,
                        EncounterDifficulty.Medium => 2000,
                        EncounterDifficulty.Hard => 3000,
                        _ => 4500,
                    };
                case 13: //13th	1100	2200	3400	5100
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 1100,
                        EncounterDifficulty.Medium => 2200,
                        EncounterDifficulty.Hard => 3400,
                        _ => 5100,
                    };
                case 14: //14th	1250	2500	3800	5700
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 1250,
                        EncounterDifficulty.Medium => 2500,
                        EncounterDifficulty.Hard => 3800,
                        _ => 5700,
                    };
                case 15: //15th	1400	2800	4300	6400
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 1400,
                        EncounterDifficulty.Medium => 2800,
                        EncounterDifficulty.Hard => 4300,
                        _ => 6400,
                    };
                case 16:  //16th	1600	3200	4800	7200
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 1600,
                        EncounterDifficulty.Medium => 3200,
                        EncounterDifficulty.Hard => 4800,
                        _ => 7200,
                    };
                case 17: //17th	2000	3900	5900	8800
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 2000,
                        EncounterDifficulty.Medium => 3900,
                        EncounterDifficulty.Hard => 5900,
                        _ => 8800,
                    };
                case 18:  //18th	2100	4200	6300	9500
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 2100,
                        EncounterDifficulty.Medium => 4200,
                        EncounterDifficulty.Hard => 6300,
                        _ => 9500,
                    };
                case 19: //19th	2400	4900	7300	10900
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 2400,
                        EncounterDifficulty.Medium => 4900,
                        EncounterDifficulty.Hard => 7300,
                        _ => 10900,
                    };
                case 20: //20th	2800	5700	8500	12700
                    return difficulty switch
                    {
                        EncounterDifficulty.Easy => 2800,
                        EncounterDifficulty.Medium => 5700,
                        EncounterDifficulty.Hard => 8500,
                        _ => 12700,
                    };

                default: Debug.LogError(QcLog.CaseNotImplemented(Value, nameof(GetExperienceThresholdFor))); return 10;
            }

            
        }

        public void Inspect()
        {
            var val = Value;
            if (pegi.edit(ref val))
                Value = val;

        }
    }

    public enum EncounterDifficulty { Easy, Medium, Hard, Deadly }

}
