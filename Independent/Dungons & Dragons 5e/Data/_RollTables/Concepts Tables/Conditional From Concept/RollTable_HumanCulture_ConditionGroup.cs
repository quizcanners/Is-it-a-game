using QuizCanners.Inspect;
using UnityEngine;


namespace Dungeons_and_Dragons.Tables
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = TABLE_CREATE_NEW_PATH + "Concept/" + FILE_NAME)]
    public class RollTable_HumanCulture_ConditionGroup : DnD_RollTable_ConceptConditionGeneric<Human_Culture>
    {
        public const string FILE_NAME = "Conditional Table Group "+nameof(Human_Culture);
    }
    [PEGI_Inspector_Override(typeof(RollTable_HumanCulture_ConditionGroup))] internal class RollTable_HumanCulture_ConditionGroupDrawer : PEGI_Inspector_Override { }

}