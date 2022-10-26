using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public partial class ParticlePhisics
    {
        public struct PositionData : IPEGI_ListInspect
        {
            public Vector3 Position;

            public void InspectInList(ref int edited, int index)
            {
                "Pos".PegiLabel(40).Edit(ref Position);
            }
        }

        public struct UpwardImpulse :  IPEGI_ListInspect
        {
            public Vector3 Position;
            public Vector3 Direction;
            public float EnergyLeft;

            public void InspectInList(ref int edited, int index)
            {
                "Pos".PegiLabel(40).Edit(ref Position);
                "Energy".PegiLabel(40).Edit(ref EnergyLeft);
            }
        }

        public struct HeatedSmokeData :  IPEGI_ListInspect
        {
            public float Temperature;
            public float Dissolve;
            
            public void InspectInList(ref int edited, int index)
            {
                "Temperatire".PegiLabel(90).Edit(ref Temperature);
                "Dissolve".PegiLabel(50).Edit_01(ref Dissolve);
            }

        }

        public struct AffectedByWind : IPEGI_ListInspect
        {
            public float Buoyancy;

            public void InspectInList(ref int edited, int index)
            {
                "Thickness".PegiLabel(50).Edit(ref Buoyancy);
            }
        }

        public struct HeatSource :  IPEGI_ListInspect
        {
            public float Temperature;

            public void InspectInList(ref int edited, int index)
            {
                "Temperatire".PegiLabel(90).Edit(ref Temperature);
            }
        }

        public struct HeatImpulse :  IPEGI_ListInspect
        {
            public float ImpulseExpansion01;
            public float MaxHeat;
            public bool IsExpanding;

            public void InspectInList(ref int edited, int index)
            {
                "Expansion".PegiLabel(90).Edit_01(ref ImpulseExpansion01);
            }
        }

        public void Inspect(IEntity entity)
        {
            entity.InspectComponent<PositionData>();
        }
    }
}