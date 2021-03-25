using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    public enum Drop { None = 0, Highest = 1, Lowest = 2 }

   

    public enum Dice
    {
       D2 = 2, D4 = 4, D6 = 6, D8 = 8, D10 = 10, D12 = 12, D20 = 20, D100 = 100
    }
    

    public static class DiceUtils
    {
        public static string GetName(this Dice dice) => 'd' + ((int)dice).ToString();

        public static RollResult Roll(this List<Dice> list, RanDndSeed seed, int seedModifier)
        {
            using (seed.SetDisposible(seedModifier))
            {
                return list.Roll();
            }
        }

        public static RollResult Roll(this List<Dice> list)
        {
            RollResult sum = new();

            if (!list.IsNullOrEmpty())
                foreach (var dice in list)
                    sum += dice.Roll();
            
            return sum;
        }
        
        public static RollResult MinRoll(this List<Dice> list)
        {
            if (list.IsNullOrEmpty())
                return new RollResult();
                
            return RollResult.From(list.Count);
        }
        
        public static RollResult MaxRoll(this List<Dice> list)
        {
            if (list.IsNullOrEmpty())
                return new RollResult();

            RollResult max = new();
            
            foreach(var el in list)
                max += (int)el;
            
            return max;
        }

        public static string ToDescription(this List<Dice> list, int bonus)
        {
            if (list.Count == 0)
                return Mathf.Max(1, bonus).ToString(); ;

            int avg = Mathf.Max(1, list.AvargeRoll() + bonus);

            string bonusTxt = bonus > 0 ? (" + {0}".F(bonus)) : (bonus == 0 ? "" : bonus.ToString());

            return "{0} ({1}d{2}{3})".F(avg, list.Count, (int)list[0], bonusTxt);
        }

        public static string ToRollTableDescription(this List<Dice> list, bool showPossibiliesNumber = false)
        {    
            if (list.IsNullOrEmpty())
                return "NO DICE";
            
            StringBuilder sb = new(list.Count * 4);

            var tmpList = new List<Dice>(list);

            int ind = 0;

            while (tmpList.Count>0)
            {
                if (ind>0)
                    sb.Append('+');

                ind++;

                var el = tmpList[0];
                int cnt = tmpList.CountOccurances(el);
                tmpList.RemoveAll(d => d == el);

                if (cnt > 1)
                    sb.Append(cnt);

                sb.Append(el.GetName());
            }
            
            if (list.Count > 1 && showPossibiliesNumber) 
            {
                sb.Append('=')
                    .Append(list.MaxRoll())
                    .Append(" (")
                    .Append((list.MaxRoll() - list.MinRoll() + 1).ToString())
                    .Append(" values)");
            }

            return sb.ToString();
        }
        
        public static RollResult Roll(this Dice dice, RollInfluence influence, int diceCount = 1)
        {
            if (influence == RollInfluence.None)
                return dice.Roll(diceCount);

            RollResult sum = new();

            Drop drop = influence == RollInfluence.Advantage ? Drop.Lowest : Drop.Highest;

            for (int i = 0; i < diceCount; i++)
            {
                sum += dice.Roll(diceCount: 2, drop);
            }

            return sum;
        }

        public static RollResult Roll(this Dice dice, int diceCount, Drop drop)
        {
            if (drop == Drop.None)
                return dice.Roll(diceCount);

            RollResult sum = new();
            RollResult toDrop = new();

            for (int i = 0; i < diceCount; i++)
            {
                RollResult roll = Roll(dice);

                bool needToDrop = toDrop == 0 || (drop == Drop.Highest && roll > toDrop) || (drop == Drop.Lowest && roll < toDrop);

                if (needToDrop)
                {
                    sum += toDrop;
                    toDrop = roll;
                }
                else
                {
                    sum += roll;
                }
            }

            return sum;
        }

        public static RollResult Roll(this Dice dice, int diceCount = 1)
        {
            int value = (int)dice;

            if (diceCount == 1)
                return Roll_Internal(value);

            RollResult sum = new();

            for (int i = 0; i < diceCount; i++)
            {
                sum += Roll_Internal(value);
            }

            return sum;
        }

        private static RollResult Roll_Internal(int value) 
            => RollResult.From(Random.Range(minInclusive: 1, maxExclusive: value + 1));

        public static int AvargeRoll(this Dice dice, int diceCount = 1) =>
             ((int)dice * diceCount + diceCount) / 2;

        public static int AvargeRoll(this List<Dice> dices) 
        {
            int sum = 0;

            foreach (var d in dices)
                sum += (int)d + 1;

            return sum / 2;
        }

        public static List<int> CalculateRollResultProbabilities(this List<Dice> dices)
        {
            if (dices.Count == 0)
                return new List<int>(); ;

            int min = dices.MinRoll().Value;

            int cnt = dices.MaxRoll().Value - min + 1;
            var lst = new List<int>(cnt);

            for (int i = 0; i < cnt; i++)
                lst.Add(0);

            LoopInternal(0, 0);

            void LoopInternal(int i_iteration, int i_sum) 
            {
                if (i_iteration>= dices.Count) 
                {
                    lst[i_sum - min] += 1;
                } else 
                {
                    Dice loop = dices[i_iteration];

                    int max = (int)loop;

                    for (int i=1; i<=max; i++) 
                    {
                        LoopInternal(i_iteration + 1, i_sum + i);
                    }
                }
            }

            return QcMath.NormalizeToPercentage(lst, x => x);
        }

        public static int CountOccurances(this List<Dice> dices, Dice diceToCount) 
        {
            if (dices.IsNullOrEmpty())
                return 0;

            int count = 0;
            foreach (var d in dices)
                if (d == diceToCount)
                    count++;

            return count;
        }

        public static int MaxRoll(this Dice dice, int diceCount = 1) => (int)dice * diceCount;
    }
}

