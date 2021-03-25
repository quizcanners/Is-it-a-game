using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class AdventureNodeNpcCharacter : IPEGI, IPEGI_ListInspect
    {
        [SerializeField] protected NpcCharacter.SmartId npcId = new();

        public void Inspect() => npcId.Nested_Inspect();

        public void InspectInList(ref int edited, int index) => npcId.InspectInList(ref edited, index);
        
        
    }
}
