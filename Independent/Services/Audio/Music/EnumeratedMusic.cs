using UnityEngine;
using QuizCanners.Inspect;


namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/Managers/Audio/" + FILE_NAME)]

    public class EnumeratedMusic : EnumeratedAssetListsBase<IigEnum_Music, MusicClipData>
    {
        public const string FILE_NAME = "Enumerated Music";


    }


    [PEGI_Inspector_Override(typeof(EnumeratedMusic))] internal class CoreLocatorEnumeratedMusicDrawer : PEGI_Inspector_Override { }

}