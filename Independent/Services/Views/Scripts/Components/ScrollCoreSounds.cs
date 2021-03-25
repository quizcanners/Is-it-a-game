using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuizCanners.IsItGame
{
    public class ScrollCoreSounds : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {

        public ScrollRect scrollRect;

        private float dragged;
        public void TryResetInertia() => scrollRect.velocity = Vector2.zero;

        [NonSerialized] public bool dragging;

        [Header("Config")]
        public bool playScrollSound = true;

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }

        private void Reset()
        {
            scrollRect = GetComponent<ScrollRect>();
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (playScrollSound)
            {
                dragged += eventData.delta.magnitude;
                if (dragged > 50)
                {
                    dragged = 0;

                    if (scrollRect && (scrollRect.vertical || scrollRect.horizontal))
                    {
                        IigEnum_SoundEffects.Scratch.Play(); 
                    }
                }
            }
        }
    }
}
