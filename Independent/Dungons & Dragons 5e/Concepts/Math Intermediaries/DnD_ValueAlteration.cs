using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public enum ValueAlteration { None, Halved, Doubled, CancelledOut, Reduced_To_Zero }

    public enum RollInfluence { None = 0, Disadvantage = 1, Advantage = 2, Cancelled = 3 }

    public static class ValueAlterationExtensions 
    {
        public static int ApplyTo(this ValueAlteration alteration, int value) 
        {
            switch (alteration) 
            {
                case ValueAlteration.Reduced_To_Zero: return 0;
                case ValueAlteration.Doubled: return value * 2;
                case ValueAlteration.Halved: return Mathf.FloorToInt(value * 0.5f);
                case ValueAlteration.None: 
                case ValueAlteration.CancelledOut:
                default:
                    
                    return value;
            }
        }

        public static ValueAlteration And(this ValueAlteration A, ValueAlteration B)
        {
            if (A == ValueAlteration.Reduced_To_Zero || B == ValueAlteration.Reduced_To_Zero)
                return ValueAlteration.Reduced_To_Zero;

            if (A == ValueAlteration.None)
                return B;

            if (B == ValueAlteration.None)
                return A;

            if (A == B)
                return A;

            return ValueAlteration.CancelledOut;
        }

        public static RollInfluence And(this RollInfluence A, RollInfluence B)
        {
            if (A == RollInfluence.Cancelled || B == RollInfluence.Cancelled)
                return RollInfluence.Cancelled;

            if (A == RollInfluence.None)
                return B;

            if (B == RollInfluence.None)
                return A;

            if (A == B)
                return A;

            return RollInfluence.Cancelled;
        }
    }
}