using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    [Serializable]
    public class DiceCalculator : IPEGI
    {
        [SerializeField] private DiceDictionary _dices = new();
        [SerializeField] private int _toAdd;

        [Serializable]
        private class DiceDictionary : SerializableDictionary<Dice, DiceRolls> {}

        [Serializable]
        private class DiceRolls 
        {
            [SerializeField] private int _count;
            public List<RollResult> Rolls = new();
            public int Sum { get; private set; } = 0;

            public int Count => _count;
            public void Reroll(Dice dice) 
            {
                Rolls.Clear();
                Sum = 0;
                for (int i = 0; i < _count; i++)
                {
                    var dr = dice.Roll();
                    Rolls.Add(dr);
                    Sum += dr.Value;
                }
            }

            public void SetCount(int count, Dice dice) 
            {
                _count = count;
                Reroll(dice);
            }
        }

        private int Total 
        {
            get 
            {
                int total = 0;
                foreach (var r in _dices)
                    total += r.Value.Sum;

                return total;
            }
        }

        private int MinValue 
        {
            get 
            {
                int min = 0;
                foreach (var el in _dices)
                    min += el.Value.Count;

                return min + _toAdd;
            }
        }

        private int MaxValue
        {
            get
            {
                int max = 0;
                foreach (var el in _dices)
                    max += el.Key.MaxRoll(el.Value.Count);

                return max + _toAdd;
            }
        }

        private DiceRolls GetRollsFor(Dice dice) => _dices.GetOrCreate(dice);

        private int this[Dice dice] 
        {
            get => _dices.GetOrCreate(dice).Count;
            set 
            {
                if (value <= 0)
                    _dices.Remove(dice);
                else
                {
                    _dices[dice].SetCount(value, dice);
                }
            }
        }

        public void Inspect()
        {
            Total.ToString().PegiLabel(width: 50, style: pegi.Styles.BaldText).write();

            "+".PegiLabel(15).edit(ref _toAdd, 30);

            "= {0}".F(Total + _toAdd).PegiLabel(width: 60, style: pegi.Styles.HeaderText).write();

            if (MinValue > 0)
            {
                " [{0}-{1}]".F(MinValue, MaxValue).PegiLabel(width: 80, style: pegi.Styles.BaldText).write();

                if (icon.Dice.Click())
                    foreach (var d in _dices)
                        d.Value.Reroll(d.Key);

                if (icon.Clear.Click())
                {
                    _dices.Clear();
                    _toAdd = 0;
                }
            }

            pegi.nl();

            Click(Dice.D2); Click(Dice.D4); Click(Dice.D6); pegi.nl();
            Click(Dice.D8); Click(Dice.D10); Click(Dice.D12); pegi.nl();
            Click(Dice.D20); Click(Dice.D100); pegi.nl();

            void Click(Dice dice) 
            {
                int cnt = this[dice];
                (cnt>0 ? "{0} (x{1})".F(dice.GetName(), cnt) : dice.GetName()).PegiLabel().Click(() => this[dice] += 1);
            }


            foreach (var d in (Dice[])Enum.GetValues(typeof(Dice))) 
            {
                var rolls = GetRollsFor(d);
                int count = rolls.Count;

                if (pegi.edit_WithButtons(ref count, 50))
                    this[d] = count;

                if (count > 0) {

                    "{0}{1}= {2}".F(count, d.GetName(), rolls.Sum).PegiLabel(width: 120, style: pegi.Styles.BaldText).write();

                    icon.Clear.Click(() => this[d] = 0);
                    icon.Dice.Click(() => rolls.Reroll(d));

                    if (count > 1)
                    {
                        var sb = new System.Text.StringBuilder();
                        foreach (var r in rolls.Rolls)
                            sb.Append(r.Value).Append("  ");

                        pegi.nl();
                        
                        sb.ToString().PegiLabel(style: pegi.Styles.OverflowText).nl();
                    }

                } else 
                {
                    d.GetName().PegiLabel(120).write();
                }
                pegi.nl();
            }

        }
    }
}