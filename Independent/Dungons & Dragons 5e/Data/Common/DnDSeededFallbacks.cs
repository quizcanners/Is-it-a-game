using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class SeededFallacks : IPEGI
    {
        [SerializeField] private RandomElementsRollTables _name;
        [SerializeField] private RandomElementsRollTables _race;
        [SerializeField] private RandomElementsRollTables _class;


        public string GetName(Creature creature, string defautValue) => _name ? _name.GetRolledElementName(creature.Seed, creature, shortText: true) : defautValue;
        public Race GetRace(Creature creature) => GetFallback_Internal(_race, creature, Race.Human);
        public Class GetClass(Creature creature) => GetFallback_Internal(_class, creature, Class.Fighter);

        private T GetFallback_Internal<T>(RandomElementsRollTables table, Creature creature, T defaultValue) 
        {
            if (table && table.TryGetConcept(out T value, creature.Seed, creature))
                return value;

            return defaultValue;
        }


        [SerializeField] private pegi.EnterExitContext _inspectedFallback = new();
        public void Inspect()
        {
            using (_inspectedFallback.StartContext())
            {
                "Name".PegiLabel().edit_enter_Inspect(ref _name).nl();
                "Race".PegiLabel().edit_enter_Inspect(ref _race).nl();
                "Class".PegiLabel().edit_enter_Inspect(ref _class).nl();
            }
        }
    }
}
