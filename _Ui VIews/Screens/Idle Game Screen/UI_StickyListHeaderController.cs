using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizCanners.IsItGame.UI
{

    [DisallowMultipleComponent]
    public class UI_StickyListHeaderController : MonoBehaviour
    {
        [SerializeField] private RectTransform _scrolledContent;
        [SerializeField] private RectTransform _myContent;
        [SerializeField] private RectTransform _header;

        [SerializeField] private List<Graphic> _graphicsToFade;
        [SerializeField] private List<Graphic> _graphicsToShowWhenOffset;
        private float PosY 
        {
            get => _header.anchoredPosition.y;
            set
            {
                _header.anchoredPosition = _header.anchoredPosition.Y(value);
            }
        }

        private readonly Gate.Float _positionChange = new();

        private void LateUpdate()
        {
            var up = _scrolledContent.anchoredPosition.y;

            if (_positionChange.TryChange(up))
            {
                var offset = _myContent.anchoredPosition.y;

                var screenPosition = up + offset;

                var height = _header.rect.height;

                float maxOffset = _myContent.rect.height - height;

                PosY = -Mathf.Clamp(screenPosition, 0, maxOffset);

                if (_graphicsToFade.Count > 0)
                {
                    float alpha = Mathf.Clamp01((maxOffset - screenPosition ) / height);
                    _graphicsToFade.TrySetAlpha(alpha);
                }

                if (_graphicsToShowWhenOffset.Count > 0) 
                {
                    float showAlpha = (-PosY) / height;
                    _graphicsToShowWhenOffset.TrySetAlpha(showAlpha);
                }
            }

        }
    }
}
