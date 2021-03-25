using QuizCanners.Inspect;
using UnityEngine;
using QuizCanners.Migration;

namespace QuizCanners.SpecialEffects
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + SpecialEffectShadersService.ASSEMBLY_NAME + "/Managers/" + FILE_NAME)]
    public class SceneLighting_Configurations : ConfigurationsSO_Generic<WeatherConfig>, IPEGI
    {
        public const string FILE_NAME = "Scene Lighting Config";
    }

    public class WeatherConfig : Configuration
    {
        public static Configuration activeConfig;

        protected override Configuration ActiveConfig_Internal
        {
            get { return activeConfig; }
            set
            {
                activeConfig = value;
                SceneLightingManager.inspected.Decode(value);
            }
        }

        public override CfgEncoder EncodeData() => SceneLightingManager.inspected.Encode();

    }
}