using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class C_ImpactLightForTopDown : MonoBehaviour, IPEGI
    {
        private const float DEFAULT_SIZE = 16;
        private float _targetSize;
        private bool _scalingUp;

        private float _sizePortion;
        private float SizePortion 
        {
            get => _sizePortion;
            set 
            {
                _sizePortion = value;
                transform.localScale = Vector3.one * _targetSize * value;
            }
        }

        public void SetSize(float size = DEFAULT_SIZE) 
        {
            _targetSize = size;
            SizePortion = 0.1f;
            _scalingUp = true;
        }

        void LateUpdate() 
        {
            float speed = Mathf.Lerp(3f, 1f , Singleton.TryGetValue<Pool_ImpactLightsController, float>(s => s.VacancyPortion, 1));

            SizePortion = LerpUtils.LerpBySpeed(SizePortion, _scalingUp ? 1 : 0.01f, speed * (_scalingUp ? 50 : 20f), unscaledTime: false);

            if (SizePortion >= 1)
                _scalingUp = false;

            if (SizePortion < 0.02f)
                Pool_ImpactLightsController.ReturnToPool(this);
        }

        public void Inspect()
        {
        }
    }

    [PEGI_Inspector_Override(typeof(C_ImpactLightForTopDown))] internal class C_ImpactLightForTopDownDrawer : PEGI_Inspector_Override { }
}