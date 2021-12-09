using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.Triggers.Dialogue;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class NpcCharacter : IPEGI_ListInspect, IGotName, IPEGI
    {
        [SerializeField] private string _name = "UNNAMED";
        public CharacterSheet.SmartId SheetId = new();
        public Dialogue Dialogue;

        #region Inspector
        public string NameForInspector { get => _name; set => _name = value; }

        public virtual void InspectInList(ref int edited, int index)
        {
            "Key".PegiLabel(40).editDelayed(ref _name);

            SheetId.InspectInList(ref edited, index);
        }

        private int _inspectedStuff = -1;
        public virtual void Inspect()
        {
            int entered = -1;
            "Dialogue".PegiLabel().edit_enter_Inspect(ref Dialogue, ref _inspectedStuff, ++entered).nl();
            SheetId.enter_Inspect_AsList(ref _inspectedStuff, ++entered).nl();
        }

        #endregion


        [Serializable]
        public class SmartId : SmartStringIdGeneric<NpcCharacter>
        {
            protected override System.Collections.Generic.Dictionary<string, NpcCharacter> GetEnities() => GameController.instance.EntityPrototypes.NPCCharacters.AllNpcs;
        }
    }

   
}