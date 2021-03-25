using QuizCanners.Inspect;
using QuizCanners.IsItGame.Develop;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class EntityPrototypes : IsItGameScriptableObjectBase, IPEGI
    {
        public const string FILE_NAME = "Entity Prototypes";

        public NpcCharacterCollectionsScriptableObject NPCCharacters;

        #region Inspector

        private int _inspecteedStuff = -1;
        public void Inspect()
        {
            int category = -1;
            "NPCs".PegiLabel().edit_enter_Inspect(ref NPCCharacters, ref _inspecteedStuff, ++category).nl();
        }
        #endregion
    }
}