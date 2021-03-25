using QuizCanners.SpecialEffects;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class GyroscopeTestView : MonoBehaviour
    {
        [SerializeField] private RectTransform _gyroOffset;

        private void Update()
        {
            Singleton.Try<SpecialEffectShadersService>(serv => _gyroOffset.anchoredPosition = serv.GyroscopeParallax.AccumulatedOffset * 512, logOnServiceMissing: false);
        }
    }
}
