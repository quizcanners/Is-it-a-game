using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;



namespace QuizCanners.IsItGame.Develop
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class SO_ECSParticlesConfig : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "ECS Settings";



        public Vector3 Wind;

        public void Inspect()
        {
            "Wind".PegiLabel(50).Edit(ref Wind).Nl();
        }
    }
}
