using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [System.Serializable]
    public struct RollResult
    {
        [SerializeField] private int _value;

        public int Value
        {
            set => _value = value;
            get
            {
                return _value;
            }
        }

        public override string ToString() => _value.ToString();

        public static bool operator > (RollResult rollA, RollResult rollB) => rollA._value > rollB._value;
        public static bool operator < (RollResult rollA, RollResult rollB) => rollA._value < rollB._value;
        public static bool operator >= (RollResult rollA, RollResult rollB) => rollA._value >=  rollB._value;
        public static bool operator <= (RollResult rollA, RollResult rollB) => rollA._value <= rollB._value;
        public static bool operator == (RollResult rollA, RollResult rollB) => rollA._value == rollB._value;
        public static bool operator != (RollResult rollA, RollResult rollB) => rollA._value != rollB._value;


        public static bool operator >(RollResult rollA, int val) => rollA._value > val;
        public static bool operator <(RollResult rollA, int val) => rollA._value < val;
        public static bool operator >=(RollResult rollA, int val) => rollA._value >= val;
        public static bool operator <=(RollResult rollA, int val) => rollA._value <= val;
        public static bool operator ==(RollResult rollA, int val) => rollA._value == val;
        public static bool operator !=(RollResult rollA, int val) => rollA._value != val;

        public override bool Equals(object obj)
        {
            return obj is RollResult result &&
                   _value == result._value;
        }

        public override int GetHashCode()
        {
            int hashCode = -917080737;
            hashCode = hashCode * -1521134295 + _value.GetHashCode();
            return hashCode;
        }

        public static RollResult operator +(RollResult rollA, RollResult rollB)
        {
            rollA._value += rollB._value;
            return rollA;
        }
        public static RollResult operator +(RollResult roll, int bonus)
        {
            roll._value += bonus;
            return roll;
        }
        public static RollResult operator -(RollResult rollA, RollResult rollB)
        {
            rollA._value = Mathf.Max(0, rollA._value - rollB._value);
            return rollA;
        }
        public static RollResult operator -(RollResult roll, int penalty)
        {
            roll._value = Mathf.Max(0, roll._value - penalty);
            return roll;
        }

        public static RollResult From(int value)
        {
            var r = new RollResult();
            r._value = value;
            return r;
        }
    }
}