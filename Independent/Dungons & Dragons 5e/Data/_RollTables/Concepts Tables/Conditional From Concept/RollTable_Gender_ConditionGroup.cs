using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = TABLE_CREATE_NEW_PATH + "Concept/" + FILE_NAME)]
    public class RollTable_Gender_ConditionGroup : DnD_RollTable_ConceptConditionGeneric<Gender>
    {
        public const string FILE_NAME = "Conditional Table Group " + nameof(Gender);
    }
    [PEGI_Inspector_Override(typeof(RollTable_Gender_ConditionGroup))] internal class RollTable_Gender_ConditionGroupDrawer : PEGI_Inspector_Override { }
}