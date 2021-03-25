using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Exhaustion
    {
        [SerializeField]
        private int _level;
        public int Level 
        {
            get => _level;
            set 
            {
                _level = Mathf.Clamp(value, 0, 6);
            }
        }

        public ValueAlteration Speed => Level >= 2 ? (Level >=5 ? ValueAlteration.Reduced_To_Zero : ValueAlteration.Halved) : ValueAlteration.None;

        public ValueAlteration HitPointMaximum => Level >= 4 ? ValueAlteration.Halved : ValueAlteration.None;

        public RollInfluence this[KindOfD20Roll kind] 
        {
            get 
            {
                switch (kind) 
                {
                    case KindOfD20Roll.Ability_Check: return Level >= 1 ? RollInfluence.Disadvantage : RollInfluence.None;
                    default: return Level >= 3 ? RollInfluence.Disadvantage : RollInfluence.None;
                }
            }
        }

        public bool Death => Level >= 6;

    }
}
