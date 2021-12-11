using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayableCharacterState : IGotName
    {
        public string Name;

        public Dungeons_and_Dragons.CharacterState MainHero = new Dungeons_and_Dragons.CharacterState();

        public string NameForInspector { get => Name; set => Name = value; }
    }

    [Serializable]
    public class DictionaryOfPlayableCharacterStates : SerializableDictionary<string, PlayableCharacterState> {}

    public class PlayableCharacterId : SmartStringIdGeneric<PlayableCharacterState>
    {
        protected override Dictionary<string, PlayableCharacterState> GetEnities() =>
            Singleton.Get<Singleton_GameController>().PersistentProgressData.User.Campaign.PlayableCharacters;
    }

}
