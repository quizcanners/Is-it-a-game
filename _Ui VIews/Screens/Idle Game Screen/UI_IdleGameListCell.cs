using QuizCanners.SpecialEffects;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class UI_IdleGameListCell : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        private RectTransformTiltMgmt _tiltMgmt = new RectTransformTiltMgmt();

        private void Update()
        {
            _tiltMgmt.UpdateTilt(rect, Camera.current, mouseEffectRadius: 0.5f, speed: 100, maxTilt: 30);
        }
    }
}
