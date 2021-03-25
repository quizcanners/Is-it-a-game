using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = TABLE_CREATE_NEW_PATH + "Concept/" + FILE_NAME)]
    public class RollTable_Skill_Concept : RollTable_ForEnum_Generic<Skill>
    {
        public const string FILE_NAME = "Random "+nameof(Skill);
    }

    [PEGI_Inspector_Override(typeof(RollTable_Skill_Concept))] internal class RollTable_Skill_ConceptDrawer : PEGI_Inspector_Override { }
}