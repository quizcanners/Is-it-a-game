using QuizCanners.SpecialEffects;
using QuizCanners.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace QuizCanners.IsItGame.UI
{
    public class UI_IdleGameListCell : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Button _clickButton;
        [SerializeField] private SO_CurrencyAnimationPrototype _currency;
        private RectTransformTiltMgmt _tiltMgmt = new RectTransformTiltMgmt();


        int testValue = 0;

        private void Awake()
        {
            if (_clickButton)
            _clickButton.onClick.AddListener(()=> 
            {
                Singleton.Try<Pool_CurrencyAnimationController>(s =>
                {
                    testValue++;
                    s.RequestAnimation(_currency, rect, targetValue: s.GetTargetValue(_currency) + testValue * testValue);
                });
            });
        }

        private void Update()
        {
            _tiltMgmt.UpdateTilt(rect, Camera.current, mouseEffectRadius: 0.5f, speed: 100, maxTilt: 30);
        }
    }
}
