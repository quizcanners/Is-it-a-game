using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class NpcCharacterCollectionsScriptableObject : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Game Characters";
        [SerializeField] public DictionaryOfNpcs AllNpcs = new DictionaryOfNpcs();

        #region Inspector

        private int _inspectedNpc = -1;
        public void Inspect()
        {
            "NPCs".PegiLabel().edit_Dictionary(AllNpcs, ref _inspectedNpc).nl();

           
        }
        #endregion

        [Serializable] public class DictionaryOfNpcs : SerializableDictionary<string, NpcCharacter>{}
    }

    [PEGI_Inspector_Override(typeof(NpcCharacterCollectionsScriptableObject))] internal class CharactersScriptableObjectDrawer : PEGI_Inspector_Override { }
}
