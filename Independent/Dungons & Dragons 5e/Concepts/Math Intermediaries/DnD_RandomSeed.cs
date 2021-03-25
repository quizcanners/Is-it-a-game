using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class RanDndSeed
    {
        [SerializeField] private bool _initialized;
        [SerializeField] private int _value;

        public int Value 
        {
            get 
            {
                if (!_initialized)
                    Randomize();

                return _value;
            }
            set 
            {
                _initialized = true;
                _value = value;
            }
        }

        public IDisposable SetDisposible(int modifier) => QcMath.RandomBySeed(Value + modifier);

        public int Randomize() => Value = (int)(UnityEngine.Random.value * int.MaxValue);
    }
}