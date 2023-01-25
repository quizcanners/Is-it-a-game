using QuizCanners.SpecialEffects;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class View_GyroscopeTest : UI_TypedView
    {
        [SerializeField] private RectTransform _gyroOffset;

        public override Game.Enums.View MyView => Game.Enums.View.Gyroscope;

        private void Update()
        {
            Singleton.Try<Singleton_SpecialEffectShaders>(serv => _gyroOffset.anchoredPosition = serv.GyroscopeParallax.AccumulatedOffset * 512, logOnServiceMissing: false);
        }
    }
}
