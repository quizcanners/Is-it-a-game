using QuizCanners.TinyECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public partial class ParticlePhisics
    {
        public struct PositionData : IComponentData
        {
            public Vector3 Position;
        }

        public struct SmokeData : IComponentData
        {
            public float Temperature;
        }

        public struct HeatSource : IComponentData 
        {
            public float Temperature;
        }

        public struct HeatImpulse : IComponentData 
        {
            public float ImpulseExpansion01;
            public float MaxHeat;
            public bool IsExpanding;
        }

        public void Inspect(IEntity entity)
        {
            entity.InspectComponent<PositionData>();
        }
    }
}