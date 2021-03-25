using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = TABLE_CREATE_NEW_PATH + "Concept/" + FILE_NAME)]
    public class RollTable_Class_Concept : RollTable_ForEnum_Generic<Class>
    {
        public const string FILE_NAME = "Random "+nameof(Class);
    }

    [PEGI_Inspector_Override(typeof(RollTable_Class_Concept))] internal class RollTable_Class_ConceptDrawer : PEGI_Inspector_Override { }
}