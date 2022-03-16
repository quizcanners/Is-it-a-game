using QuizCanners.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace QuizCanners.IsItGame.SpaceEffect
{
    public class UI_SpaceAndStarsScrollController : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private float horisontalSpeed = 0.02f;
        [SerializeField] private float verticalSpeed = 0.02f;


        private void Reset()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void LateUpdate()
        {
            if (_scrollRect && _scrollRect.content)
            {
                Singleton.Try<Singleton_SpaceAndStars>(onFound: s =>
                {
                    if (_scrollRect.horizontal) 
                    {
                        s.HorisontalOffset = ((float)_scrollRect.content.anchoredPosition.x) / Screen.height * horisontalSpeed;
                    }

                    if (_scrollRect.vertical)
                    {
                        s.VericalOffset = ((float)_scrollRect.content.anchoredPosition.y) / Screen.width * verticalSpeed;
                    }
                }, logOnServiceMissing: false);
            }
        }
    }
}
