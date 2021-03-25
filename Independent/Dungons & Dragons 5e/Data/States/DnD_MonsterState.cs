using System;
using QuizCanners.Inspect;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class MonsterState : CreatureStateGeneric<Monster>, IPEGI, IPEGI_ListInspect
    {
        public Monster.SmartId MonsterId = new();

        public override DnD_SmartId<Monster> CreatureId => MonsterId;

    }
}