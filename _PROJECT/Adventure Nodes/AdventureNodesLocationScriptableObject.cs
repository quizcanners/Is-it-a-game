using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class AdventureNodesLocationScriptableObject : ScriptableObject, IPEGI, IGotName
    {
        public const string FILE_NAME = "Adventure Location";
  
        public List<TableRollResult> RandomRollResults = new();
        public List<AdventureNodeSkillCheck> SkillChecks = new();
        public List<AdventureNodeNpcCharacter> NpcCharacters = new();

        [SerializeField] private string _key;

        #region Inspector
        private readonly pegi.EnterExitContext _context = new();
        private readonly pegi.CollectionInspectorMeta _rollsMeta = new("Random Roll Results"); // _inspectedTableRollResult = -1;
        private readonly pegi.CollectionInspectorMeta _inspectedSkill = new("Skill Checks");
        private readonly pegi.CollectionInspectorMeta _inspectedNpc = new("Npc Characters");

        public string NameForInspector { get => _key; set => _key = value; }

        public void Inspect()
        {
            pegi.nl();

            using (_context.StartContext())
            {
                _rollsMeta.enter_List(RandomRollResults).nl();
                _inspectedSkill.enter_List(SkillChecks).nl();
                _inspectedNpc.enter_List(NpcCharacters).nl();
            }
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(AdventureNodesLocationScriptableObject))] internal class AdventureNodesLocationDrawer : PEGI_Inspector_Override { }
}