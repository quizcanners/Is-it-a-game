using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class SO_AdventureNodesLocation : ScriptableObject, IPEGI, IGotName
    {
        public const string FILE_NAME = "Adventure Location";
  
        public List<TableRollResult> RandomRollResults = new();
        public List<SO_AdventureNodeSkillCheckCfg> SkillChecks = new();

        [SerializeField] private string _key;

        #region Inspector
        private readonly pegi.EnterExitContext _context = new();
        private readonly pegi.CollectionInspectorMeta _rollsMeta = new("Random Roll Results"); // _inspectedTableRollResult = -1;
        private readonly pegi.CollectionInspectorMeta _inspectedSkill = new("Skill Checks");

        public string NameForInspector { get => _key; set => _key = value; }

        public void Inspect()
        {
            pegi.Nl();

            using (_context.StartContext())
            {
                _rollsMeta.Enter_List(RandomRollResults).Nl();
                _inspectedSkill.Enter_List(SkillChecks).Nl();
            }
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(SO_AdventureNodesLocation))] internal class SO_AdventureNodesLocationDrawer : PEGI_Inspector_Override { }
}