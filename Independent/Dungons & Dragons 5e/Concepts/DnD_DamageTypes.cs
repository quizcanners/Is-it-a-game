using UnityEngine;

namespace Dungeons_and_Dragons
{

    public enum DamageType
    {
        Acid,
        Bludgeoning,
        Cold, 
        Fire,
        Force,
        Lightning,
        Necrotic,
        Piercing,
        Poison,
        Psychic,
        Radiant,
        Slashing,
        Thunder,
    }

    public enum DamageResistance
    {
        None, Resistance, Vulnerability, CancelledOut , Immunity
    }

    public static class DamageResistanceExtensions 
    {

        public static int ModifyDamage(this DamageResistance res, int damage)
        {
            switch (res) 
            {
                case DamageResistance.Resistance: return Mathf.FloorToInt(damage / 2f);
                case DamageResistance.Vulnerability: return damage * 2;
                case DamageResistance.Immunity: return 0;
                case DamageResistance.CancelledOut:
                case DamageResistance.None:
                default: return damage;
            }
        }

        public static DamageResistance Add(this DamageResistance a, DamageResistance b) 
        {
            if (Any(DamageResistance.Immunity))
                return DamageResistance.Immunity;

            if (Any(DamageResistance.CancelledOut))
                return DamageResistance.CancelledOut;

            if (a == b)
                return a;

            if (a == DamageResistance.None)
                return b;
            
            if (b == DamageResistance.None)
                return a;

            return DamageResistance.CancelledOut;

            bool Any(DamageResistance type) 
            {
                return a == type || b == type;
            }

        }
    }

}
