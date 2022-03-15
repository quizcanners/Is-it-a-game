using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_ImpactLightForTopDown : MonoBehaviour, IPEGI
    {
        public void Restart() 
        {

            transform.localScale = Vector3.one * 2;
        }

        void LateUpdate() 
        {
            float size = transform.localScale.x;

            LerpUtils.IsLerpingBySpeed(ref size, 0.01f, 5);

            transform.localScale = Vector3.one * size;

            if (size < 0.02f)
                Singleton_ImpactLightsController.OnFinished(this);
        }

        public void Inspect()
        {



        }
    }

    [PEGI_Inspector_Override(typeof(C_ImpactLightForTopDown))] internal class C_ImpactLightForTopDownDrawer : PEGI_Inspector_Override { }
}