using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.SpaceEffect
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = Utils.QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/Test/" + FILE_NAME)]
    public class SO_SpaceAndStarsConfigs : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Space Background Configurations";

        [SerializeField] private ConfigsDictionary dic = new ConfigsDictionary();

        public void PlayFirst() 
        {
            if (dic.Count>0)
            {
                dic.GetElementAt(0).Value.SetSelected();
            }
        }

        public void Play(string key) 
        {
            if (dic.TryGetValue(key, out var val)) 
            {
                val.SetSelected();
            }
        }

        [System.Serializable] private class ConfigsDictionary : SerializableDictionary<string, SpaceAndStarsConfiguration> 
        {
            protected override string ElementName => "mushroom.planet.name";
        }

        public void Inspect() =>  dic.Nested_Inspect();     
    }

    [PEGI_Inspector_Override(typeof(SO_SpaceAndStarsConfigs))] internal class SpaceAndStarsConfigsScriptableObjecDrawer : PEGI_Inspector_Override { }
}

