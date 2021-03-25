using UnityEngine;
using QuizCanners.Inspect;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/Managers/Audio/" + FILE_NAME)]
    public class EnumeratedSounds : EnumeratedAssetListsBase<IigEnum_SoundEffects, AudioClip>
    {
        public const string FILE_NAME = "Enumerated Sounds";


    }

    [PEGI_Inspector_Override(typeof(EnumeratedSounds))] internal class CoreLocatorEnumeratedSoundsDrawer : PEGI_Inspector_Override { }

}
