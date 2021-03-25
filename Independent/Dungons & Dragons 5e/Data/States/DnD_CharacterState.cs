using QuizCanners.Inspect;
using System;
using System.Collections.Generic;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class CharacterState : CreatureStateGeneric<CharacterSheet>, IPEGI
    {

        public List<Weapon.SmartId> Weapons = new();

        public CharacterSheet.SmartId CharacterId = new();

        public override List<Attack> GetAttacksList()
        {
            var lst = base.GetAttacksList();

            var prot = Creature;
            if (prot != null) 
            {
                foreach (var w in Weapons)
                    if (Creature.TryGetAttack(w, out var attack))
                        lst.Add(attack);
            }

            return lst;
        }

        public override DnD_SmartId<CharacterSheet> CreatureId => CharacterId;

        protected override void Inspect_Context() 
        {
            "Weapons".PegiLabel().enter_List(Weapons).nl();
        }

    }
}
