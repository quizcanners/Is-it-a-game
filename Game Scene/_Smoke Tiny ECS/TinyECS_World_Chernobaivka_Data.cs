using QuizCanners.Inspect;
using QuizCanners.TinyECS;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public partial class ParticlePhisics
    {
        public struct PositionData : IComponentData, IPEGI_ListInspect
        {
            public Vector3 Position;

            public void InspectInList(ref int edited, int index)
            {
                "Pos".PegiLabel(40).Edit(ref Position);
            }
        }

        public struct SmokeData : IComponentData, IPEGI_ListInspect
        {
            public float Temperature;
            public float Dissolve;

            public void InspectInList(ref int edited, int index)
            {
                "Temperatire".PegiLabel(90).Edit(ref Temperature);
                "Dissolve".PegiLabel(50).Edit(ref Dissolve);
            }
        }

        public struct HeatSource : IComponentData , IPEGI_ListInspect
        {
            public float Temperature;

            public void InspectInList(ref int edited, int index)
            {
                "Temperatire".PegiLabel(90).Edit(ref Temperature);
            }
        }

        public struct HeatImpulse : IComponentData , IPEGI_ListInspect
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