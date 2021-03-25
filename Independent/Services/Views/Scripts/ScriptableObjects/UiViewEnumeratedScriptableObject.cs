using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/Managers/" + FILE_NAME)]
    public class UiViewEnumeratedScriptableObject : EnumeratedAssetReferences<IigEnum_UiView, GameObject>
    {
        public const string FILE_NAME = "Enumerated Views";
    }


    [PEGI_Inspector_Override(typeof(UiViewEnumeratedScriptableObject))] internal class UiViewEnumeratedScriptableObjectDrawer : PEGI_Inspector_Override { }
}
